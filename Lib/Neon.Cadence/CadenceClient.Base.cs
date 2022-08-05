﻿//-----------------------------------------------------------------------------
// FILE:	    CadenceClient.Base.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright (c) 2005-2022 by neonFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Neon.Cadence;
using Neon.Cadence.Internal;
using Neon.Common;
using Neon.Tasks;

namespace Neon.Cadence
{
    public partial class CadenceClient
    {
        //---------------------------------------------------------------------
        // Cadence basic client related operations.

        private AsyncMutex workerRegistrationMutex = new AsyncMutex();

        /// <summary>
        /// Pings the <b>cadence-proxy</b> and waits for the reply.  This is used 
        /// mainly for low-level performance and load testing but can also be used
        /// to explicitly verify that the <b>cadence-proxy</b> is still alive.
        /// </summary>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public async Task PingAsync()
        {
            await SyncContext.Clear;
            EnsureNotDisposed();

            await CallProxyAsync(new PingRequest());
        }

        /// <summary>
        /// Scans the assembly passed looking for workflow and activity implementations 
        /// derived from and registers them with Cadence.  This is equivalent to calling
        /// <see cref="RegisterAssemblyWorkflowsAsync(Assembly, string)"/> and
        /// <see cref="RegisterAssemblyActivitiesAsync(Assembly, string)"/>,
        /// </summary>
        /// <param name="assembly">The target assembly.</param>
        /// <param name="domain">Optionally overrides the default client domain.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <exception cref="TypeLoadException">
        /// Thrown for types tagged by <see cref="WorkflowAttribute"/> that are not 
        /// derived from <see cref="WorkflowBase"/> or for types tagged by <see cref="ActivityAttribute"/>
        /// that are now derived from <see cref="ActivityBase"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown if one of the tagged classes conflict with an existing registration.</exception>
        /// <exception cref="ActivityWorkerStartedException">
        /// Thrown if an activity worker has already been started for the client.  You must
        /// register activity implementations before starting workers.
        /// </exception>
        /// <exception cref="WorkflowWorkerStartedException">
        /// Thrown if a workflow worker has already been started for the client.  You must
        /// register workflow implementations before starting workers.
        /// </exception>
        /// <remarks>
        /// <note>
        /// Be sure to register all services you will be injecting into activities via
        /// <see cref="NeonHelper.ServiceContainer"/> before you call this as well as 
        /// registering of your activity and workflow implementations before starting 
        /// workers.
        /// </note>
        /// </remarks>
        public async Task RegisterAssemblyAsync(Assembly assembly, string domain = null)
        {
            await SyncContext.Clear;
            EnsureNotDisposed();
            
            await RegisterAssemblyWorkflowsAsync(assembly, domain);
            await RegisterAssemblyActivitiesAsync(assembly, domain);
        }

        /// <summary>
        /// Signals Cadence that the application is capable of executing workflows and/or activities for a specific
        /// domain and task list.
        /// </summary>
        /// <param name="taskList">Specifies the task list implemented by the worker.  This must not be empty.</param>
        /// <param name="options">Optionally specifies additional worker options.</param>
        /// <param name="domain">Optionally overrides the default <see cref="CadenceClient"/> domain.</param>
        /// <returns>A <see cref="Worker"/> identifying the worker instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="taskList"/> is <c>null</c> or empty.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when an attempt is made to recreate a worker with the
        /// same properties on a given client.  See the note in the remarks.
        /// </exception>
        /// <remarks>
        /// <note>
        /// <see cref="CadenceClient"/> for more information on task lists.
        /// </note>
        /// <para>
        /// Your workflow application will need to call this method so that Cadence will know
        /// that it can schedule activities to run within the current process.  You'll need
        /// to specify the target Cadence domain and task list.
        /// </para>
        /// <para>
        /// You may also specify an optional <see cref="WorkerOptions"/> parameter as well
        /// as customize the name used to register the activity, which defaults to the
        /// fully qualified name of the activity type.
        /// </para>
        /// <para>
        /// This method returns a <see cref="Worker"/> which implements <see cref="IDisposable"/>.
        /// It's a best practice to call <see cref="Dispose()"/> just before the a worker process
        /// terminates, but this is optional.  Advanced worker implementation that need to change
        /// their configuration over time can also call <see cref="Dispose()"/> to stop workers
        /// for specific domains and task lists.
        /// </para>
        /// <note>
        /// The Cadence GOLANG client does not appear to support starting a worker with a given
        /// set of parameters, stopping that workflow, and then restarting another worker
        /// with the same parameters on the same client.  This method detects this situation
        /// and throws an <see cref="InvalidOperationException"/> when a restart is attempted.
        /// </note>
        /// </remarks>
        public async Task<Worker> StartWorkerAsync(string taskList, WorkerOptions options = null, string domain = null)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(taskList), nameof(taskList), "Workers must be started with a non-empty task list.");
            EnsureNotDisposed();

            options  = options ?? new WorkerOptions();
            domain   = ResolveDomain(domain);

            WorkerMode  mode = options.Mode;
            Worker      worker;

            try
            {
                using (await workerRegistrationMutex.AcquireAsync())
                {
                    // Ensure that we haven't already registered a worker for the
                    // specified activity, domain, and task list.  We'll just increment
                    // the reference count for the existing worker and return it 
                    // in this case.
                    //
                    // I know that this is a linear search but the number of activity
                    // registrations per service will generally be very small and 
                    // registrations will happen infrequently (typically just once
                    // per service, when it starts).

                    // $note(jefflill):
                    //
                    // If the worker exists but its RefCount==0, then we're going to
                    // throw an exception because Cadence doesn't support recreating
                    // a worker with the same parameters on the same client.

                    worker = workers.Values.SingleOrDefault(wf => wf.Mode == mode && wf.Domain == domain && wf.Tasklist == taskList);

                    if (worker != null)
                    {
                        if (worker.RefCount < 0)
                        {
                            throw new InvalidOperationException("A worker with these same parameters has already been started and stopped on this Cadence client.  Cadence does not support recreating workers for a given client instance.");
                        }

                        Interlocked.Increment(ref worker.RefCount);
                        return worker;
                    }

                    options          = options ?? new WorkerOptions();
                    options.Identity = this.Settings.ClientIdentity;

                    var reply = (NewWorkerReply)(await CallProxyAsync(
                        new NewWorkerRequest()
                        {
                            Domain   = domain,
                            TaskList = taskList,
                            Options  = options.ToInternal()
                        }));

                    reply.ThrowOnError();

                    worker = new Worker(this, mode, reply.WorkerId, domain, taskList);
                    workers.Add(reply.WorkerId, worker);
                }
            }
            finally
            {
                switch (mode)
                {
                    case WorkerMode.Activity:

                        activityWorkerStarted = true;
                        break;

                    case WorkerMode.Workflow:

                        workflowWorkerStarted = true;
                        break;

                    case WorkerMode.Both:

                        activityWorkerStarted = true;
                        workflowWorkerStarted = true;
                        break;

                    default:

                        throw new NotImplementedException();
                }
            }

            return worker;
        }

        /// <summary>
        /// Returns information about pollers (AKA workers) that have communicated 
        /// with the Cadence cluster in the last few minutes.
        /// </summary>
        /// <param name="taskList">Identifies the tasklist.</param>
        /// <param name="taskListType">
        /// Indicates whether to return information for decision (AKA workflow pollers)
        /// or activity pollers.
        /// </param>
        /// <param name="domain">Optionally specifies the Cadence domain.</param>
        /// <returns>The <see cref="TaskListDescription"/> for the pollers.</returns>
        public async Task<TaskListDescription> DescribeTaskListAsync(string taskList, TaskListType taskListType, string domain = null)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(taskList), nameof(taskList));
            EnsureNotDisposed();

            domain = ResolveDomain(domain);

            var reply = (DescribeTaskListReply)await CallProxyAsync(
                new DescribeTaskListRequest()
                {
                    Name         = taskList,
                    TaskListType = taskListType,
                    Domain       = domain
                });

            reply.ThrowOnError();

            return reply.Result.ToPublic();
        }

        //---------------------------------------------------------------------
        // Internal utilities

        /// <summary>
        /// Signals Cadence that it should stop invoking activities and workflows 
        /// for the specified <see cref="Worker"/> (returned by a previous call to
        /// <see cref="StartWorkerAsync(string, WorkerOptions, string)"/>).
        /// </summary>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <remarks>
        /// This method does nothing if the worker is already stopped.
        /// </remarks>
        internal async Task StopWorkerAsync(Worker worker)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(worker != null, nameof(worker));
            EnsureNotDisposed(noClosingCheck: true);

            using (await workerRegistrationMutex.AcquireAsync())
            {
                if (!object.ReferenceEquals(worker.Client, this))
                {
                    throw new InvalidOperationException("The worker passed does not belong to this client connection.");
                }

                if (!workers.ContainsKey(worker.WorkerId))
                {
                    // The worker does not exist.  We're going to ignore this.

                    return;
                }

                // $note(jefflill):
                //
                // If Cadence was able to restart a given worker, we'd uncomment
                // this line.

                // workers.Remove(worker.WorkerId);
            }

            var reply = (StopWorkerReply)(await CallProxyAsync(new StopWorkerRequest() { WorkerId = worker.WorkerId }));
            
            reply.ThrowOnError();
        }
    }
}
