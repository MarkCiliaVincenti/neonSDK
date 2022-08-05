﻿//-----------------------------------------------------------------------------
// FILE:	    WorkflowInterfaceAttribute.cs
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
using System.Reflection;

using Neon.Common;
using Neon.Temporal;
using Neon.Temporal.Internal;

namespace Neon.Temporal
{
    /// <summary>
    /// Used to tag workflow interfaces and optionally specify the task queue
    /// identifying the workers hosting this workflow.  <see cref="TemporalClient"/>
    /// for more information on how task lists work.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class WorkflowInterfaceAttribute : Attribute
    {
        private string      @namespace;
        private string      taskQueue;

        /// <summary>
        /// Constructor.
        /// </summary>
        public WorkflowInterfaceAttribute()
        {
        }

        /// <summary>
        /// Optionally specifies the Temporal namespace where the workflow is registered.
        /// </summary>
        public string Namespace
        {
            get => @namespace;

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    @namespace = null;
                }
                else
                {
                    @namespace = value;
                }
            }
        }

        /// <summary>
        /// Optionally specifies the Temporal task queue identifying the workers
        /// hosting this workflow.
        /// </summary>
        public string TaskQueue
        {
            get => taskQueue;

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    taskQueue = null;
                }
                else
                {
                    taskQueue = value;
                }
            }
        }
    }
}
