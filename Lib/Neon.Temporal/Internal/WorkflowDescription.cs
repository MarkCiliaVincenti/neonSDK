﻿//-----------------------------------------------------------------------------
// FILE:	    WorkflowDescription.cs
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

using Newtonsoft.Json;

using Neon.Common;
using Neon.Temporal;
using Neon.Temporal.Internal;

namespace Neon.Temporal.Internal
{
    /// <summary>
    /// Describes a workflow execution.
    /// </summary>
    public class WorkflowDescription
    {
        /// <summary>
        /// Describes the workflow's configuration.
        /// </summary>
        [JsonProperty(PropertyName = "execution_config")]
        public WorkflowExecutionConfig ExecutionConfig { get; set; }

        /// <summary>
        /// Describes the workflow's execution details.
        /// </summary>
        [JsonProperty(PropertyName = "workflow_execution_info")]
        public WorkflowExecutionInfo WorkflowExecutionInfo { get; set; }

        /// <summary>
        /// Describes the workflow's scheduled and executing activities.
        /// </summary>
        [JsonProperty(PropertyName = "pending_activities")]
        public List<PendingActivityInfo> PendingActivities { get; set; } = new List<PendingActivityInfo>();

        /// <summary>
        /// Describes the workflow's scheduled and executing child workflows.
        /// </summary>
        [JsonProperty(PropertyName = "pending_children")]
        public List<PendingChildExecutionInfo> PendingChildren { get; set; } = new List<PendingChildExecutionInfo>();
    }
}
