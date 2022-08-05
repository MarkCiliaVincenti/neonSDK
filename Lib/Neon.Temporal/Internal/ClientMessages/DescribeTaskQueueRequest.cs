﻿//-----------------------------------------------------------------------------
// FILE:	    DescribeTaskQueueRequest.cs
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
    /// <b>client --> proxy:</b> Requests a list of the Temporal namespaces.
    /// </summary>
    [InternalProxyMessage(InternalMessageTypes.DescribeTaskQueueRequest)]
    internal class DescribeTaskQueueRequest : ProxyRequest
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public DescribeTaskQueueRequest()
        {
            Type = InternalMessageTypes.DescribeTaskQueueRequest;
        }

        /// <inheritdoc/>
        public override InternalMessageTypes ReplyType => InternalMessageTypes.DescribeTaskQueueReply;

        /// <summary>
        /// Identifies the task queue.
        /// </summary>
        public string Name
        {
            get => GetStringProperty(PropertyNames.Name);
            set => SetStringProperty(PropertyNames.Name, value);
        }

        /// <summary>
        /// Identifies the target namespace.
        /// </summary>
        public string Namespace
        {
            get => GetStringProperty(PropertyNames.Namespace);
            set => SetStringProperty(PropertyNames.Namespace, value ?? string.Empty); 
        }

        /// <summary>
        /// Identifies the type of task queue being requested: decision (AKA workflow or activity).
        /// </summary>
        public TaskQueueType TaskQueueType
        {
            get => GetEnumProperty<TaskQueueType>(PropertyNames.TaskQueueType);
            set => SetEnumProperty<TaskQueueType>(PropertyNames.TaskQueueType, value);
        }

        /// <inheritdoc/>
        internal override ProxyMessage Clone()
        {
            var clone = new DescribeTaskQueueRequest();

            CopyTo(clone);

            return clone;
        }

        /// <inheritdoc/>
        protected override void CopyTo(ProxyMessage target)
        {
            base.CopyTo(target);

            var typedTarget = (DescribeTaskQueueRequest)target;

            typedTarget.Name          = this.Name;
            typedTarget.TaskQueueType = this.TaskQueueType;
        }
    }
}
