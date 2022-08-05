﻿//-----------------------------------------------------------------------------
// FILE:	    NewWorkerRequest.cs
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

using Neon.Common;
using Neon.Temporal;

namespace Neon.Temporal.Internal
{
    /// <summary>
    /// <b>client --> proxy:</b> Creates a new worker but doesn't start it.
    /// </summary>
    [InternalProxyMessage(InternalMessageTypes.NewWorkerRequest)]
    internal class NewWorkerRequest : ProxyRequest
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public NewWorkerRequest()
        {
            Type = InternalMessageTypes.NewWorkerRequest;
        }

        /// <inheritdoc/>
        public override InternalMessageTypes ReplyType => InternalMessageTypes.NewWorkerReply;

        /// <summary>
        /// Optionally specifies the Temporal namespace for the worker.  This defaults to
        /// <see cref="TemporalSettings.Namespace"/>.
        /// </summary>
        public string Namespace
        {
            get => GetStringProperty(PropertyNames.Namespace);
            set => SetStringProperty(PropertyNames.Namespace, value);
        }

        /// <summary>
        /// <para>
        /// Optionally specifies the Temporal task queue for the worker.  This defaults to
        /// <see cref="TemporalSettings.TaskQueue"/>.
        /// </para>
        /// <note>
        /// You must ensure that this is not <c>null</c> or empty.
        /// </note>
        /// </summary>
        public string TaskQueue
        {
            get => GetStringProperty(PropertyNames.TaskQueue);
            set => SetStringProperty(PropertyNames.TaskQueue, value);
        }

        /// <summary>
        /// The worker options.
        /// </summary>
        public WorkerOptions Options
        {
            get => GetJsonProperty<WorkerOptions>(PropertyNames.Options);
            set => SetJsonProperty<WorkerOptions>(PropertyNames.Options, value);
        }

        /// <inheritdoc/>
        internal override ProxyMessage Clone()
        {
            var clone = new NewWorkerRequest();

            CopyTo(clone);

            return clone;
        }

        /// <inheritdoc/>
        protected override void CopyTo(ProxyMessage target)
        {
            base.CopyTo(target);

            var typedTarget = (NewWorkerRequest)target;

            typedTarget.Namespace = this.Namespace;
            typedTarget.Options   = this.Options;
            typedTarget.TaskQueue = this.TaskQueue;
            typedTarget.Options   = this.Options;
        }
    }
}
