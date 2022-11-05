﻿//-----------------------------------------------------------------------------
// FILE:	    ActivityInterfaceAttribute.cs
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
using System.Diagnostics.Contracts;
using System.Reflection;

using Neon.Cadence;
using Neon.Cadence.Internal;
using Neon.Common;

namespace Neon.Cadence
{
    /// <summary>
    /// Used to tag activity interfaces and optionally specify the task list
    /// identifying the workers hosting this activity.  <see cref="CadenceClient"/>
    /// for more information on how task lists work.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class ActivityInterfaceAttribute : Attribute
    {
        private string      domain;
        private string      taskList;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ActivityInterfaceAttribute()
        {
        }

        /// <summary>
        /// Optionally specifies the Cadence domain where the activity is registered.
        /// </summary>
        public string Domain
        {
            get => domain;

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    domain = null;
                }
                else
                {
                    domain = value;
                }
            }
        }

        /// <summary>
        /// Optionally specifies the Cadence task list identifying the workers
        /// hosting this activity.
        /// </summary>
        public string TaskList
        {
            get => taskList;

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    taskList = null;
                }
                else
                {
                    taskList = value;
                }
            }
        }
    }
}
