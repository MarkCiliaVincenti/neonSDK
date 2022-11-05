﻿//-----------------------------------------------------------------------------
// FILE:	    WorkflowQueueCloseRequest.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright © 2005-2022 by NEONFORGE LLC.  All rights reserved.
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

using Neon.Cadence;
using Neon.Common;

namespace Neon.Cadence.Internal
{
    /// <summary>
    /// <b>proxy --> client:</b> Closes a workflow queue.
    /// </summary>
    [InternalProxyMessage(InternalMessageTypes.WorkflowQueueCloseRequest)]
    internal class WorkflowQueueCloseRequest : WorkflowRequest
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public WorkflowQueueCloseRequest()
        {
            Type = InternalMessageTypes.WorkflowQueueCloseRequest;
        }

        /// <inheritdoc/>
        public override InternalMessageTypes ReplyType => InternalMessageTypes.WorkflowQueueCloseReply;

        /// <summary>
        /// Identifies the queue.
        /// </summary>
        public long QueueId
        {
            get => GetLongProperty(PropertyNames.QueueId);
            set => SetLongProperty(PropertyNames.QueueId, value);
        }

        /// <inheritdoc/>
        internal override ProxyMessage Clone()
        {
            var clone = new WorkflowQueueCloseRequest();

            CopyTo(clone);

            return clone;
        }

        /// <inheritdoc/>
        protected override void CopyTo(ProxyMessage target)
        {
            base.CopyTo(target);

            var typedTarget = (WorkflowQueueCloseRequest)target;

            typedTarget.QueueId = this.QueueId;
        }
    }
}
