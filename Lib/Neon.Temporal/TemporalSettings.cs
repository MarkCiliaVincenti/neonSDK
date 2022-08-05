﻿//-----------------------------------------------------------------------------
// FILE:	    TemporalSettings.cs
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
using System.Text;

using Newtonsoft.Json;
using YamlDotNet.Serialization;

using Neon.Common;
using Neon.Diagnostics;
using Neon.Temporal.Internal;
using Neon.Net;

namespace Neon.Temporal
{
    /// <summary>
    /// <para>
    /// Temporal client settings.
    /// </para>
    /// <note>
    /// Temporal settings properties are considered to be <b>read/write</b> until they
    /// are used for connecting a <see cref="TemporalClient"/>.  After that time,
    /// these properties are considered <b>read-only</b> and an <see cref="InvalidOperationException"/>
    /// will be thrown when you try to set a property value.
    /// </note>
    /// </summary>
    public class TemporalSettings
    {
        private const double defaultTimeoutSeconds = 24 * 3600;

        /// <summary>
        /// Set to the associated client.  Settings are considered to be <b>read-only</b>
        /// when this is not <c>null</c>.
        /// </summary>
        internal TemporalClient Client { get; set; }

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> when <see cref="Client"/>
        /// is not <c>null</c>.
        /// </summary>
        private void EnsureReadWrite()
        {
            if (Client != null)
            {
                throw new InvalidOperationException($"[{nameof(TemporalSettings)} is read-only after being used to establish a Temporal client connection.");
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="hostPort">
        /// Optionally specifies the target server host and port being connected.
        /// Specifies the Temporal server host and port being connected.  This is typically formatted
        /// as <b>host:port</b> where <b>host</b> is the IP address or hostname for the
        /// Temporal server.  Alternatively, this can be formatted as <b>dns:///host:port</b>
        /// to enable DNS round-robin lookups.  This defaults to <b>localhost:7233</b>.
        /// </param>
        public TemporalSettings(string hostPort = "localhost:7233")
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(hostPort), nameof(hostPort));

            this.HostPort = hostPort;
        }

        /// <summary>
        /// Specifies the Temporal server host and port being connected.  This is typically formatted
        /// as <b>host:port</b> where <b>host</b> is the IP address or hostname for the
        /// Temporal server.  Alternatively, this can be formatted as <b>dns:///host:port</b>
        /// to enable DNS round-robin lookups.  This defaults to <b>localhost:7233</b>.
        /// </summary>
        [JsonProperty(PropertyName = "HostPort", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "hostPort", ApplyNamingConventions = false)]
        [DefaultValue("localhost:7233")]
        public string HostPort
        {
            get => hostPort;

            set
            {
                EnsureReadWrite();
                hostPort = value;
            }
        }

        private string hostPort = "localhost:7233";

        /// <summary>
        /// Optionally specifies the port where the client will listen for traffic from the 
        /// associated <b>temporal-proxy</b>.  This defaults to 0 which specifies that lets the 
        /// operating system choose an unused ephermal port.
        /// </summary>
        [JsonProperty(PropertyName = "ListenPort", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "listenPort", ApplyNamingConventions = false)]
        [DefaultValue(0)]
        public int ListenPort
        {
            get => listenPort;

            set
            {
                EnsureReadWrite();

                if (!NetHelper.IsValidPort(value) && value != 0)
                {
                    throw new ArgumentException($"[{value}] is not a valid network port.", nameof(ListenPort));
                }

                listenPort = value;
            }
        }

        private int listenPort = 0;

        /// <summary>
        /// Specifies the default Temporal namespace for this client.  This is required and defaults to <c>"default"</c>.
        /// </summary>
        /// <remarks>
        /// The default namespace can be overridden for workflow interfaces via <see cref="WorkflowInterfaceAttribute.Namespace"/>
        /// or for specific interface methods via <see cref="WorkflowMethodAttribute.Namespace"/>.
        /// </remarks>
        [JsonProperty(PropertyName = "Namespace", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "namespace", ApplyNamingConventions = false)]
        [DefaultValue("default")]
        public string Namespace
        {
            get => @namespace;

            set
            {
                EnsureReadWrite();
                @namespace = value;
            }
        }

        private string @namespace = "default";

        /// <summary>
        /// <para>
        /// Optionally create the <see cref="Namespace"/> if it doesn't already exist.
        /// This defaults to <c>false</c>.
        /// </para>
        /// <note>
        /// Enabling this can be handy for unit testing where you'll likely be starting
        /// off with a virgin Temporal server when the test start.  We don't recommend
        /// enabling this for production services.  For production, you should explicitly
        /// create your namespaces with suitable setttings such has how long workflow histories
        /// are to be retained.
        /// </note>
        /// <note>
        /// If the default namespace doesn't exist when <see cref="CreateNamespace"/><c>=true</c> when a
        /// connection is established, it will be initialized to retain workflow histories for
        /// up to <b>7 days</b>.
        /// </note>
        /// </summary>
        [JsonProperty(PropertyName = "CreateNamespace", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "createNamespace", ApplyNamingConventions = false)]
        [DefaultValue(false)]
        public bool CreateNamespace
        {
            get => createNamespace;

            set
            {
                EnsureReadWrite();
                createNamespace = value;
            }
        }

        private bool createNamespace = false;

        /// <summary>
        /// Specifies the default Temporal task queue for this client.  This is optional and defaults to <c>"default"</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Specifying a default task queue can be convienent for many scenarios, especially for those where
        /// the application workflows and activities are restricted to a single task queue.
        /// </para>
        /// <para>
        /// The default task queue can be overridden for workflow interfaces via <see cref="WorkflowInterfaceAttribute.TaskQueue"/>
        /// or for specific interface methods via <see cref="WorkflowMethodAttribute.TaskQueue"/>.
        /// </para>
        /// </remarks>
        [JsonProperty(PropertyName = "DefaultTaskQueue", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "defaultTaskQueue", ApplyNamingConventions = false)]
        [DefaultValue(null)]
        public string TaskQueue
        {
            get => defaultTaskQueue;

            set
            {
                EnsureReadWrite();
                defaultTaskQueue = value;
            }
        }

        private string defaultTaskQueue = "default";

        /// <summary>
        /// Optionally specifies the maximum time the client should wait for synchronous 
        /// operations to complete.  This defaults to <b>10 seconds</b>.
        /// </summary>
        [JsonProperty(PropertyName = "ClientTimeout", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "clientTimeout", ApplyNamingConventions = false)]
        [DefaultValue(10.0)]
        public double ClientTimeoutSeconds
        {
            get => clientTimeoutSeconds;

            set
            {
                EnsureReadWrite();

                if (value <= 0.0)
                {
                    throw new ArgumentException($"[{value}] must be greater than zero.", nameof(ClientTimeoutSeconds));
                }

                clientTimeoutSeconds = value;
            }
        }

        private double clientTimeoutSeconds = 10.0;

        /// <summary>
        /// Returns <see cref="ClientTimeoutSeconds"/> as a <see cref="TimeSpan"/>.
        /// </summary>
        internal TimeSpan ClientTimeout => TimeSpan.FromSeconds(ClientTimeoutSeconds);

        /// <summary>
        /// Optionally identifies the client application establishing the connection so that
        /// Temporal may include this in its logs and metrics.  This defaults to <b>"unknown"</b>.
        /// </summary>
        [JsonProperty(PropertyName = "ClientIdentity", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "clientIdentity", ApplyNamingConventions = false)]
        [DefaultValue("unknown")]
        public string ClientIdentity
        {
            get => clientIdentity;

            set
            {
                EnsureReadWrite();
                clientIdentity = value;
            }
        }

        private string clientIdentity = "unknown";

        /// <summary>
        /// <para>
        /// The Temporal cluster security token.  This defaults to <c>null</c>.
        /// </para>
        /// <note>
        /// This is not currently supported by the .NET Temporal client and should be
        /// left alone for now.
        /// </note>
        /// </summary>
        [JsonProperty(PropertyName = "SecurityToken", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "securityToken", ApplyNamingConventions = false)]
        [DefaultValue(null)]
        public string SecurityToken
        {
            get => securityToken;

            set
            {
                EnsureReadWrite();
                securityToken = value;
            }
        }

        private string securityToken = null;
        
        /// <summary>
        /// Optionally specifies the maximum time to allow the <b>temporal-proxy</b>
        /// to indicate that it has received a proxy request message by returning an
        /// OK response.  The proxy will be considered to be unhealthy when this 
        /// happens.  This defaults to <b>5 seconds</b>.
        /// </summary>
        [JsonProperty(PropertyName = "ProxyTimeout", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "proxyTimeout", ApplyNamingConventions = false)]
        [DefaultValue(5.0)]
        public double ProxyTimeoutSeconds
        {
            get => proxyTimeoutSeconds;

            set
            {
                EnsureReadWrite();

                if (value <= 0.0)
                {
                    throw new ArgumentException($"[{value}] must be greater than zero.", nameof(ProxyTimeoutSeconds));
                }

                proxyTimeoutSeconds = value;
            }
        }

        private double proxyTimeoutSeconds = 5.0;

        /// <summary>
        /// Returns <see cref="ProxyTimeoutSeconds"/> as a <see cref="TimeSpan"/>.
        /// </summary>
        internal TimeSpan ProxyTimeout => TimeSpan.FromSeconds(ProxyTimeoutSeconds);

        /// <summary>
        /// Optionally specifies the interval at which heartbeats are transmitted to
        /// <b>temporal-proxy</b> as a health check.  This defaults to <b>5 seconds</b>.
        /// </summary>
        [JsonProperty(PropertyName = "HeartbeatIntervalSeconds", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "heartbeatIntervalSeconds", ApplyNamingConventions = false)]
        [DefaultValue(5.0)]
        public double HeartbeatIntervalSeconds
        {
            get => heartbeatIntervalSeconds;

            set
            {
                EnsureReadWrite();

                if (value <= 0.0)
                {
                    throw new ArgumentException($"[{value}] must be greater than zero.", nameof(HeartbeatIntervalSeconds));
                }

                heartbeatIntervalSeconds = value;
            }
        }

        private double heartbeatIntervalSeconds = 5.0;

        /// <summary>
        /// Returns <see cref="HeartbeatIntervalSeconds"/> as a <see cref="TimeSpan"/>.
        /// </summary>
        internal TimeSpan HeartbeatInterval => TimeSpan.FromSeconds(HeartbeatIntervalSeconds);

        /// <summary>
        /// Optionally specifies the maximum time to allow the <b>temporal-proxy</b>
        /// to respond to a heartbeat message.  The proxy will be considered to be 
        /// unhealthy when this happens.  This defaults to <b>5 seconds</b>.
        /// </summary>
        [JsonProperty(PropertyName = "HeartbeatTimeoutSeconds", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "heartbeatTimeoutSeconds", ApplyNamingConventions = false)]
        [DefaultValue(5.0)]
        public double HeartbeatTimeoutSeconds
        {
            get => heartbeatTimeoutSeconds;

            set
            {
                EnsureReadWrite();

                if (value <= 0.0)
                {
                    throw new ArgumentException($"[{value}] must be greater than zero.", nameof(HeartbeatTimeoutSeconds));
                }

                heartbeatTimeoutSeconds = value;
            }
        }

        private double heartbeatTimeoutSeconds = 5.0;

        /// <summary>
        /// Returns <see cref="HeartbeatTimeoutSeconds"/> as a <see cref="TimeSpan"/>.
        /// </summary>
        internal TimeSpan HeartbeatTimeout => TimeSpan.FromSeconds(HeartbeatTimeoutSeconds);

        /// <summary>
        /// Specifies the number of times to retry connecting to the Temporal cluster.  This defaults
        /// to <b>3</b>.
        /// </summary>
        [JsonProperty(PropertyName = "ConnectRetries", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "connectRetries", ApplyNamingConventions = false)]
        [DefaultValue(3)]
        public int ConnectRetries
        {
            get => connectRetries;

            set
            {
                EnsureReadWrite();

                if (value < 0.0)
                {
                    throw new ArgumentException($"[{value}] must be greater than or equal to zero.", nameof(ConnectRetries));
                }

                connectRetries = value;
            }
        }

        private int connectRetries = 3;

        /// <summary>
        /// Specifies the number of seconds to delay between cluster connection attempts.
        /// This defaults to <b>5.0 seconds</b>.
        /// </summary>
        [JsonProperty(PropertyName = "ConnectRetryDelaySeconds", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "connectRetryDelaySeconds", ApplyNamingConventions = false)]
        [DefaultValue(5.0)]
        public double ConnectRetryDelaySeconds
        {
            get => connectRetryDelaySeconds;

            set
            {
                EnsureReadWrite();

                if (value < 0.0)
                {
                    throw new ArgumentException($"[{value}] must be greater than or qeual to zero.", nameof(ConnectRetryDelaySeconds));
                }

                connectRetryDelaySeconds = value;
            }
        }

        private double connectRetryDelaySeconds = 5.0;

        /// <summary>
        /// Returns <see cref="ConnectRetryDelaySeconds"/> as a <see cref="TimeSpan"/>.
        /// </summary>
        internal TimeSpan ConnectRetryDelay => TimeSpan.FromSeconds(Math.Max(ConnectRetryDelaySeconds, 0));

        /// <summary>
        /// Specifies the default maximum workflow execution time.  This defaults to <b>24 hours</b>.
        /// </summary>
        [JsonProperty(PropertyName = "WorkflowExecutionTimeoutSeconds", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "WorkflowExecutionTimeoutSeconds", ApplyNamingConventions = false)]
        [DefaultValue(defaultTimeoutSeconds)]
        public double WorkflowExecutionTimeoutSeconds
        {
            get => workflowExecutionTimeoutSeconds;

            set
            {
                EnsureReadWrite();

                if (value <= 0.0)
                {
                    throw new ArgumentException($"[{value}] must be greater than zero.", nameof(WorkflowExecutionTimeoutSeconds));
                }

                workflowExecutionTimeoutSeconds = value;
            }
        }

        private double workflowExecutionTimeoutSeconds = defaultTimeoutSeconds;

        /// <summary>
        /// Returns <see cref="WorkflowExecutionTimeoutSeconds"/> as a <see cref="TimeSpan"/>.
        /// </summary>
        internal TimeSpan WorkflowExecutionTimeout => TimeSpan.FromSeconds(Math.Max(WorkflowExecutionTimeoutSeconds, 0));

        /// <summary>
        /// The timeout for duration of a single workflow run.
        /// The resolution is seconds.
        /// </summary>
        [JsonProperty(PropertyName = "WorkflowRunTimeoutSeconds", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "workflowRunTimeoutSeconds", ApplyNamingConventions = false)]
        [DefaultValue(defaultTimeoutSeconds)]
        public double WorkflowRunTimeoutSeconds
        {
            get => workflowRunTimeoutSeconds;

            set
            {
                EnsureReadWrite();

                if (value <= 0.0)
                {
                    throw new ArgumentException($"[{value}] must be greater than zero.", nameof(WorkflowRunTimeoutSeconds));
                }

                workflowRunTimeoutSeconds = value;
            }
        }

        private double workflowRunTimeoutSeconds = defaultTimeoutSeconds;

        /// <summary>
        /// Returns <see cref="WorkflowRunTimeoutSeconds"/> as a <see cref="TimeSpan"/>.
        /// </summary>
        internal TimeSpan WorkflowRunTimeout => TimeSpan.FromSeconds(Math.Max(WorkflowRunTimeoutSeconds, 0));

        /// <summary>
        /// Specifies the default maximum time a workflow task may execute.
        /// This must be with the range of <b>1 &lt; value &lt;= 60</b> seconds.
        /// This defaults to <b>10 seconds</b>.
        /// </summary>
        [JsonProperty(PropertyName = "WorkflowTaskTimeoutSeconds", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "workflowTaskTimeoutSeconds", ApplyNamingConventions = false)]
        [DefaultValue(10.0)]
        public double WorkflowTaskTimeoutSeconds
        {
            get => workflowTaskTimeoutSeconds;

            set
            {
                EnsureReadWrite();

                if (value < 1.0)
                {
                    throw new ArgumentException($"[{value}] must be greater than or equal to 1 second.", nameof(WorkflowTaskTimeoutSeconds));
                }

                workflowTaskTimeoutSeconds = value;
            }
        }

        private double workflowTaskTimeoutSeconds = 10.0; 

        /// <summary>
        /// Returns <see cref="WorkflowTaskTimeoutSeconds"/> as a <see cref="TimeSpan"/>.
        /// </summary>
        internal TimeSpan WorkflowTaskTimeout => TimeSpan.FromSeconds(Math.Min(Math.Max(WorkflowTaskTimeoutSeconds, 1), 60));

        /// <summary>
        /// Specifies what happens when Temporal workflows attempt to reuse workflow IDs.
        /// This defaults to <see cref="WorkflowIdReusePolicy.AllowDuplicate"/>.
        /// Workflows can customize this via <see cref="StartWorkflowOptions"/> or <see cref="ChildWorkflowOptions"/>
        /// or by setting this in the <see cref="WorkflowMethodAttribute"/> tagging the 
        /// workflow entry point method
        /// </summary>
        [JsonProperty(PropertyName = "WorkflowIdReusePolicy", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "workflowIdReusePolicy", ApplyNamingConventions = false)]
        [DefaultValue(WorkflowIdReusePolicy.AllowDuplicate)]
        public WorkflowIdReusePolicy WorkflowIdReusePolicy
        {
            get => workflowIdReusePolicy;

            set
            {
                EnsureReadWrite();
                workflowIdReusePolicy = value;
            }
        }

        private WorkflowIdReusePolicy workflowIdReusePolicy = WorkflowIdReusePolicy.AllowDuplicate;

        /// <summary>
        /// Specifies the default maximum time an activity is allowed to wait after being
        /// scheduled until it's actually scheduled to execute on a worker.  This defaults
        /// to <b>24 hours</b>.
        /// </summary>
        [JsonProperty(PropertyName = "ActivityScheduleToCloseTimeoutSeconds", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "activityScheduleToCloseTimeoutSeconds", ApplyNamingConventions = false)]
        [DefaultValue(defaultTimeoutSeconds)]
        public double ActivityScheduleToCloseTimeoutSeconds
        {
            get => activityScheduleToCloseTimeoutSeconds;

            set
            {
                EnsureReadWrite();

                if (value <= 0.0)
                {
                    throw new ArgumentException($"[{value}] must be greater than zero.", nameof(ActivityScheduleToCloseTimeoutSeconds));
                }

                activityScheduleToCloseTimeoutSeconds = value;
            }
        }

        private double activityScheduleToCloseTimeoutSeconds = defaultTimeoutSeconds;

        /// <summary>
        /// Returns <see cref="ActivityScheduleToCloseTimeoutSeconds"/> as a <see cref="TimeSpan"/>.
        /// </summary>
        internal TimeSpan ActivityScheduleToCloseTimeout => TimeSpan.FromSeconds(Math.Max(ActivityScheduleToCloseTimeoutSeconds, 0));

        /// <summary>
        /// Specifies the default maximum time an activity may run after being started.
        /// This defaults to <b>24</b> hours.
        /// </summary>
        [JsonProperty(PropertyName = "ActivityStartToCloseTimeoutSeconds", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "activityStartToCloseTimeoutSeconds", ApplyNamingConventions = false)]
        [DefaultValue(defaultTimeoutSeconds)]
        public double ActivityStartToCloseTimeoutSeconds
        {
            get => activityStartToCloseTimeoutSeconds;

            set
            {
                EnsureReadWrite();

                if (value <= 0.0)
                {
                    throw new ArgumentException($"[{value}] must be greater than zero.", nameof(ActivityStartToCloseTimeoutSeconds));
                }

                activityStartToCloseTimeoutSeconds = value;
            }
        }

        private double activityStartToCloseTimeoutSeconds = defaultTimeoutSeconds;

        /// <summary>
        /// Returns <see cref="ActivityStartToCloseTimeoutSeconds"/> as a <see cref="TimeSpan"/>.
        /// </summary>
        internal TimeSpan ActivityStartToCloseTimeout => TimeSpan.FromSeconds(Math.Max(ActivityStartToCloseTimeoutSeconds, 0));

        /// <summary>
        /// Specifies the default maximum time an activity may wait to be started after being scheduled.
        /// This defaults to <b>24</b> hours.
        /// </summary>
        [JsonProperty(PropertyName = "ActivityScheduleToStartTimeoutSeconds", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "activityScheduleToStartTimeoutSeconds", ApplyNamingConventions = false)]
        [DefaultValue(defaultTimeoutSeconds)]
        public double ActivityScheduleToStartTimeoutSeconds
        {
            get => activityScheduleToStartTimeoutSeconds;

            set
            {
                EnsureReadWrite();

                if (value <= 0.0)
                {
                    throw new ArgumentException($"[{value}] must be greater than zero.", nameof(ActivityScheduleToStartTimeoutSeconds));
                }

                activityScheduleToStartTimeoutSeconds = value;
            }
        }

        private double activityScheduleToStartTimeoutSeconds = defaultTimeoutSeconds;

        /// <summary>
        /// Returns <see cref="ActivityScheduleToStartTimeoutSeconds"/> as a <see cref="TimeSpan"/>.
        /// </summary>
        internal TimeSpan ActivityScheduleToStartTimeout => TimeSpan.FromSeconds(Math.Max(ActivityScheduleToStartTimeoutSeconds, 0));

        /// <summary>
        /// Specifies the default maximum allowed between activity heartbeats.  Activities that
        /// don't submit heartbeats within the time will be considered to be unhealthy and will
        /// be terminated.  This defaults to <b>60 seconds</b>.
        /// </summary>
        [JsonProperty(PropertyName = "ActivityHeartbeatTimeoutSeconds", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "activityHeartbeatTimeoutSeconds", ApplyNamingConventions = false)]
        [DefaultValue(60.0)]
        public double ActivityHeartbeatTimeoutSeconds
        {
            get => activityHeartbeatTimeoutSeconds;

            set
            {
                EnsureReadWrite();

                if (value <= 0.0)
                {
                    throw new ArgumentException($"[{value}] must be greater than zero.", nameof(ActivityHeartbeatTimeoutSeconds));
                }

                activityHeartbeatTimeoutSeconds = value;
            }
        }

        private double activityHeartbeatTimeoutSeconds = 60.0;

        /// <summary>
        /// Returns <see cref="ActivityHeartbeatTimeoutSeconds"/> as a <see cref="TimeSpan"/>.
        /// </summary>
        internal TimeSpan ActivityHeartbeatTimeout => TimeSpan.FromSeconds(Math.Max(ActivityHeartbeatTimeoutSeconds, 0));

        /// <summary>
        /// <b>EXPERIMENTAL:</b> Specifies the maximum seconds that a workflow will be kept alive after
        /// the workflow method returns to handle any oustanding synchronous signal queries.  This defaults
        /// to <b>30.0 seconds</b>.
        /// </summary>
        [JsonProperty(PropertyName = "MaxWorkflowKeepAliveSeconds", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "maxWorkflowKeepAliveSeconds", ApplyNamingConventions = false)]
        [DefaultValue(30.0)]
        public double MaxWorkflowKeepAliveSeconds
        {
            get => maxWorkflowKeepAliveSeconds;

            set
            {
                EnsureReadWrite();

                if (value < 0.0)
                {
                    throw new ArgumentException($"[{value}] must be greater than or equal to zero.", nameof(MaxWorkflowKeepAliveSeconds));
                }

                maxWorkflowKeepAliveSeconds = value;
            }
        }

        private double maxWorkflowKeepAliveSeconds = 30.0;

        /// <summary>
        /// Returns <see cref="MaxWorkflowKeepAliveSeconds"/> as a <see cref="TimeSpan"/>.
        /// </summary>
        internal TimeSpan MaxWorkflowKeepAlive => TimeSpan.FromSeconds(MaxWorkflowKeepAliveSeconds);

        /// <summary>
        /// The default maximum time the <see cref="TemporalClient.WaitForWorkflowStartAsync(WorkflowExecution, string, TimeSpan?)"/> method
        /// will wait for a workflow to start.  This defaults to <b>30.0 seconds</b>.
        /// </summary>
        [JsonProperty(PropertyName = "MaxWorkflowWaitUntilRunningSeconds", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "maxWorkflowWaitUntilRunningSeconds", ApplyNamingConventions = false)]
        [DefaultValue(30.0)]
        public double MaxWorkflowWaitUntilRunningSeconds
        {
            get => maxWorkflowWaitUntilRunningSeconds;

            set
            {
                EnsureReadWrite();

                if (value <= 0.0)
                {
                    throw new ArgumentException($"[{value}] must be greater than zero.", nameof(MaxWorkflowWaitUntilRunningSeconds));
                }

                maxWorkflowWaitUntilRunningSeconds = value;
            }
        }

        private double maxWorkflowWaitUntilRunningSeconds = 30.0;

        /// <summary>
        /// Returns <see cref="MaxWorkflowWaitUntilRunningSeconds"/> as a <see cref="TimeSpan"/>.
        /// </summary>
        internal TimeSpan MaxWorkflowWaitUntilRunning => TimeSpan.FromSeconds(MaxWorkflowWaitUntilRunningSeconds);

        /// <summary>
        /// Optionally specifies the folder where the embedded <b>temporal-proxy</b> binary 
        /// will be written before starting it.  This defaults to <c>null</c> which specifies
        /// that the binary will be written to the same folder where the <b>Neon.Temporal</b>
        /// assembly resides.  This folder may not be writable by the current user so this
        /// allows you to specify an alternative folder.
        /// </summary>
        [JsonProperty(PropertyName = "BinaryFolder", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "binaryFolder", ApplyNamingConventions = false)]
        [DefaultValue(null)]
        public string BinaryFolder
        {
            get => binaryFolder;

            set
            {
                EnsureReadWrite();
                binaryFolder = value;
            }
        }

        private string binaryFolder = null;

        /// <summary>
        /// <para>
        /// Optionally specifies the path to the <b>temporal-proxy</b> executable file.  This
        /// file must already be present on disk when a <see cref="TemporalClient"/> connection
        /// is established and the appropriate execute permissions must be set for Linux and
        /// OS/X.  This property takes presidence over <see cref="BinaryFolder"/> when set.
        /// </para>
        /// <para>
        /// This is useful for situations where the executable must be pre-provisioned for
        /// security.  One example is deploying Temporal workers to a Docker container with
        /// a read-only file system.
        /// </para>
        /// <note>
        /// You can use the <see cref="TemporalClient.ExtractTemporalProxy(string)"/> method to extract
        /// the Windows, Linux, and OS/X builds of the <b>temporal-proxy</b> executable from
        /// the <b>Neon.Temporal</b> assembly.
        /// </note>
        /// </summary>
        [JsonProperty(PropertyName = "BinaryPath", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "binaryPath", ApplyNamingConventions = false)]
        [DefaultValue(null)]
        public string BinaryPath
        {
            get => binaryFolder;

            set
            {
                EnsureReadWrite();
                binaryPath = value;
            }
        }

        private string binaryPath = null;

        /// <summary>
        /// Optionally specifies the logging level for the associated <b>temporal-proxy</b>.
        /// This defaults to <see cref="Neon.Diagnostics.LogLevel.None"/> which will be appropriate for most
        /// production situations.  You may wish to set this to <see cref="Neon.Diagnostics.LogLevel.Info"/>
        /// or <see cref="Neon.Diagnostics.LogLevel.Debug"/> while debugging.
        /// </summary>
        [JsonProperty(PropertyName = "LogLevel", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "logLevel", ApplyNamingConventions = false)]
        [DefaultValue(LogLevel.None)]
        public LogLevel ProxyLogLevel
        {
            get => proxyLogLevel;

            set
            {
                EnsureReadWrite();
                proxyLogLevel = value;
            }
        }

        private LogLevel proxyLogLevel = LogLevel.None;

        /// <summary>
        /// <para>
        /// Optionally specifies that low-level log messages from the embedded GOLANG Temporal client 
        /// will be included in the log output.  This defaults to <c>false</c>.
        /// </para>
        /// <note>
        /// <see cref="LogTemporal"/> controls whether log messages coming directly from the GOLANG
        /// client are included in the log output.  These messages will include things like details
        /// for connection and workflow execution failures.  <see cref="LogTemporalProxy"/> controls
        /// whether log messages from from the <b>temporal-proxy</b> code itself will be included.
        /// </note>
        /// </summary>
        [JsonProperty(PropertyName = "LogTemporal", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "logTemporal", ApplyNamingConventions = false)]
        [DefaultValue(false)]
        public bool LogTemporal
        {
            get => logTemporal;

            set
            {
                EnsureReadWrite();
                logTemporal = value;
            }
        }

        private bool logTemporal = false;

        /// <summary>
        /// <para>
        /// Optionally specifies that log messages from the internal <b>temporal-proxy</b>
        /// code that bridges between .NET and the embedded GOLANG Temporal client
        /// will be included in the log output.  This defaults to <c>false</c>.
        /// </para>
        /// <note>
        /// <see cref="LogTemporal"/> controls whether log messages coming directly from the GOLANG
        /// client are included in the log output.  These messages will include things like details
        /// for connection and workflow execution failures.  <see cref="LogTemporalProxy"/> controls
        /// whether log messages from from the <b>temporal-proxy</b> code itself will be included.
        /// </note>
        /// </summary>
        [JsonProperty(PropertyName = "LogTemporalProxy", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "logTemporalProxy", ApplyNamingConventions = false)]
        [DefaultValue(false)]
        public bool LogTemporalProxy
        {
            get => logTemporalProxy;

            set
            {
                EnsureReadWrite();
                logTemporalProxy = value;
            }
        }

        private bool logTemporalProxy = false;

        /// <summary>
        /// Optionally enable workflow logging while the workflow is being
        /// replayed from history.  This should generally be enabled only
        /// while debugging.  This defaults to <c>false</c>.
        /// </summary>
        [JsonProperty(PropertyName = "LogDuringReplay", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "logDuringReplay", ApplyNamingConventions = false)]
        [DefaultValue(false)]
        public bool LogDuringReplay
        {
            get => logDuringReplay;

            set
            {
                EnsureReadWrite();
                logDuringReplay = value;
            }
        }

        private bool logDuringReplay = false;

        /// <summary>
        /// Optionally specifies that the connection should run in DEBUG mode.  This currently
        /// launches the <b>temporal-proxy</b> with a command window (on Windows only) to make 
        /// it easier to see any output it generates and also has <b>temporal-proxy</b>.  This
        /// defaults to <c>false</c>.
        /// </summary>
        [JsonProperty(PropertyName = "Debug", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "debug", ApplyNamingConventions = false)]
        [DefaultValue(false)]
        public bool Debug
        {
            get => debug;

            set
            {
                EnsureReadWrite();
                debug = value;
            }
        }

        private bool debug = false;

        /// <summary>
        /// <b>INTERNAL USE ONLY:</b> Optionally indicates that the <b>temporal-proxy</b> will
        /// already be running for debugging purposes.  When this is <c>true</c>, the 
        /// <b>temporal-client</b> be hardcoded to listen on <b>localhost:5001</b> and
        /// the <b>temporal-proxy</b> will be assumed to be listening on <b>localhost:5000</b>.
        /// This defaults to <c>false.</c>
        /// </summary>
        [JsonIgnore]
        [YamlIgnore]
        public bool DebugPrelaunched
        {
            get => debugPrelaunched;

            set
            {
                EnsureReadWrite();
                debugPrelaunched = value;
            }
        }

        private bool debugPrelaunched = false;

        /// <summary>
        /// <b>INTERNAL USE ONLY:</b> Optionally disable health heartbeats.  This can be
        /// useful while debugging the client but should never be set for production.
        /// This defaults to <c>false</c>.
        /// </summary>
        public bool DebugDisableHeartbeats
        {
            get => debugDisableHeartbeats;

            set
            {
                EnsureReadWrite();
                debugDisableHeartbeats = value;
            }
        }

        private bool debugDisableHeartbeats = false;

        /// <summary>
        /// <b>INTERNAL USE ONLY:</b> Optionally indicates that the <b>temporal-client</b>
        /// will not perform the <see cref="InitializeRequest"/>/<see cref="InitializeReply"/>
        /// and <see cref="TerminateRequest"/>/<see cref="TerminateReply"/> handshakes 
        /// with the <b>temporal-proxy</b> for debugging purposes.  This defaults to
        /// <c>false</c>.
        /// </summary>
        [JsonIgnore]
        [YamlIgnore]
        public bool DebugDisableHandshakes
        {
            get => debugDisableHandshakes;

            set
            {
                EnsureReadWrite();
                debugDisableHandshakes = value;
            }
        }

        private bool debugDisableHandshakes = false;

        /// <summary>
        /// <b>INTERNAL USE ONLY:</b> Optionally ignore operation timeouts.  This can be
        /// useful while debugging the client but should never be set for production.
        /// This defaults to <c>false</c>.
        /// </summary>
        [JsonIgnore]
        [YamlIgnore]
        public bool DebugIgnoreTimeouts
        {
            get => debugIgnoreTimeouts;

            set
            {
                EnsureReadWrite();
                debugIgnoreTimeouts = value;
            }
        }

        private bool debugIgnoreTimeouts = false;

        /// <summary>
        /// <b>INTERNAL USE ONLY:</b> Optionally disables heartbeat handling by the
        /// emulated <b>temporal-proxy</b> for testing purposes.
        /// </summary>
        [JsonIgnore]
        [YamlIgnore]
        public bool DebugIgnoreHeartbeats
        {
            get => debugIgnoreHeartbeats;

            set
            {
                EnsureReadWrite();
                debugIgnoreHeartbeats = value;
            }
        }

        private bool debugIgnoreHeartbeats = false;

        /// <summary>
        /// <b>INTERNAL USE ONLY:</b> Optionally specifies the timeout to use for 
        /// HTTP requests made to the <b>temporal-proxy</b>.  This defaults to
        /// <b>5 seconds</b>.
        /// </summary>
        [JsonIgnore]
        [YamlIgnore]
        public TimeSpan DebugHttpTimeout
        {
            get => debugHttpTimeout;

            set
            {
                EnsureReadWrite();
                debugHttpTimeout = value;
            }
        }

        private TimeSpan debugHttpTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Returns a copy of the current instance.
        /// </summary>
        /// <returns>The cloned <see cref="TemporalSettings"/>.</returns>
        public TemporalSettings Clone()
        {
            return new TemporalSettings()
            {
                ActivityHeartbeatTimeoutSeconds       = this.ActivityHeartbeatTimeoutSeconds,
                ActivityScheduleToCloseTimeoutSeconds = this.ActivityScheduleToCloseTimeoutSeconds,
                ActivityScheduleToStartTimeoutSeconds = this.ActivityScheduleToStartTimeoutSeconds,
                ActivityStartToCloseTimeoutSeconds    = this.ActivityStartToCloseTimeoutSeconds,
                BinaryFolder                          = this.BinaryFolder,
                ClientIdentity                        = this.ClientIdentity,
                ClientTimeoutSeconds                  = this.ClientTimeoutSeconds,
                ConnectRetries                        = this.ConnectRetries,
                ConnectRetryDelaySeconds              = this.ConnectRetryDelaySeconds,
                CreateNamespace                       = this.CreateNamespace,
                Debug                                 = this.Debug,
                DebugDisableHandshakes                = this.DebugDisableHandshakes,
                DebugDisableHeartbeats                = this.DebugDisableHeartbeats,
                DebugHttpTimeout                      = this.DebugHttpTimeout,
                DebugIgnoreHeartbeats                 = this.DebugIgnoreHeartbeats,
                DebugIgnoreTimeouts                   = this.DebugIgnoreTimeouts,
                DebugPrelaunched                      = this.DebugPrelaunched,
                TaskQueue                             = this.TaskQueue,
                Namespace                             = this.Namespace,
                HeartbeatIntervalSeconds              = this.HeartbeatIntervalSeconds,
                HeartbeatTimeoutSeconds               = this.HeartbeatTimeoutSeconds,
                HostPort                              = this.HostPort,
                ListenPort                            = this.ListenPort,
                LogTemporal                           = this.LogTemporal,
                LogTemporalProxy                      = this.LogTemporalProxy,
                LogDuringReplay                       = this.LogDuringReplay,
                ProxyLogLevel                         = this.ProxyLogLevel,
                ProxyTimeoutSeconds                   = this.ProxyTimeoutSeconds,
                SecurityToken                         = this.SecurityToken,
                WorkflowIdReusePolicy                 = this.WorkflowIdReusePolicy,
                WorkflowExecutionTimeoutSeconds       = this.WorkflowExecutionTimeoutSeconds,
                WorkflowRunTimeoutSeconds             = this.WorkflowRunTimeoutSeconds,
                WorkflowTaskTimeoutSeconds            = this.WorkflowTaskTimeoutSeconds
            };
        }
    }
}
