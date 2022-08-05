﻿//-----------------------------------------------------------------------------
// FILE:	    WorkerArgs.cs
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
using Neon.Temporal.Internal;

namespace Neon.Temporal
{
    /// <summary>
    /// Holds the opaque arguments passed to <see cref="WorkflowBase"/> and <see cref="ActivityBase"/>
    /// implementations by the <see cref="TemporalClient"/> when the workflow or activity is 
    /// executed on a worker.  This must be passed to the base <see cref="WorkflowBase"/> or
    /// <see cref="ActivityBase"/> class constructors.
    /// </summary>
    public class WorkerArgs
    {
        /// <summary>
        /// The parent <see cref="Temporal.Worker"/>.
        /// </summary>
        internal Worker Worker { get; set; }

        /// <summary>
        /// The ID used to reference the corresponding Temporal context managed by
        /// the <b>temporal-proxy</b>.
        /// </summary>
        internal long ContextId { get; set; }
    }
}
