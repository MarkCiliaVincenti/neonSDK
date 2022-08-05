﻿//-----------------------------------------------------------------------------
// FILE:	    DomainConfiguration.cs
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
using Neon.Cadence.Internal;
using Neon.Common;

namespace Neon.Cadence
{
    /// <summary>
    /// Domain configuration options.
    /// </summary>
    public class DomainConfiguration
    {
        /// <summary>
        /// The workflow history retention period in days.
        /// </summary>
        public int RetentionDays { get; set; }

        /// <summary>
        /// Enables metrics for workflows and activities running in the domain.
        /// </summary>
        public bool EmitMetrics { get; set; }

        // $todo(jefflill):
        //
        // We need to add support for these additional Cadence GOLANG properties:
        //
        //      BadBinaries
        //      HistoryArchivalStatus
        //      HistoryArchivalUri
        //      VisibilityArchivalStatus
        //      VisibilityArchivalUri
    }
}
