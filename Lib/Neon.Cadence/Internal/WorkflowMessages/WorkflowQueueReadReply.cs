﻿//-----------------------------------------------------------------------------
// FILE:	    WorkflowQueueReadReply.cs
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

using Neon.Cadence;
using Neon.Common;

namespace Neon.Cadence.Internal
{
    /// <summary>
    /// <b>proxy --> client:</b> Answers a <see cref="WorkflowQueueReadRequest"/>
    /// </summary>
    [InternalProxyMessage(InternalMessageTypes.WorkflowQueueReadReply)]
    internal class WorkflowQueueReadReply : WorkflowReply
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public WorkflowQueueReadReply()
        {
            Type = InternalMessageTypes.WorkflowQueueReadReply;
        }
       
        /// <summary>
        /// Set to <c>true</c> when the queue has been closed.
        /// </summary>
        public bool IsClosed
        {
            get => GetBoolProperty(PropertyNames.IsClosed);
            set => SetBoolProperty(PropertyNames.IsClosed, value);
        }

        /// <summary>
        /// The data item read from the queue or <c>null</c> if the operation
        /// timed out or the queue has been closed.
        /// </summary>
        public byte[] Data
        {
            get => GetBytesProperty(PropertyNames.Data);
            set => SetBytesProperty(PropertyNames.Data, value);
        }

        /// <inheritdoc/>
        internal override ProxyMessage Clone()
        {
            var clone = new WorkflowQueueReadReply();

            CopyTo(clone);

            return clone;
        }

        /// <inheritdoc/>
        protected override void CopyTo(ProxyMessage target)
        {
            base.CopyTo(target);

            var typedTarget = (WorkflowQueueReadReply)target;

            typedTarget.IsClosed = this.IsClosed;
            typedTarget.Data     = this.Data;
        }
    }
}
