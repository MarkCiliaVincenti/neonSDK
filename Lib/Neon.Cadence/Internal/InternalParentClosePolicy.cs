﻿//-----------------------------------------------------------------------------
// FILE:	    InternalParentClosePolicy.cs
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
using Neon.Cadence.Internal;
using Neon.Common;

namespace Neon.Cadence.Internal
{
    /// <summary>
    /// <b>INTERNAL USE ONLY:</b> Enumerates the possible child workflow behaviors 
    /// when the parent workflow is closed.
    /// </summary>
    public enum InternalParentClosePolicy
    {
        // WARNING: These definitions must match those defined for [ParentClosePolicy].

        /// <summary>
        /// All open child workflows will be terminated when parent workflow is terminated.
        /// </summary>
        TERMINATE = 0,

        /// <summary>
        /// Cancel requests will be sent to all open child workflows to all open child 
        /// workflows when parent workflow is closed.  This is the default policy.
        /// </summary>
        REQUEST_CANCEL = 1,

        /// <summary>
        /// Child workflow execution will continue unaffected when parent workflow is
        /// closed.
        /// </summary>
        ABANDON = 2
    }
}
