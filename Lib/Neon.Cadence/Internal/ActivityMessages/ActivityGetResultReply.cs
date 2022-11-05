﻿//-----------------------------------------------------------------------------
// FILE:	    ActivityGetResultReply.cs
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
    /// <b>proxy --> client:</b> Answers a <see cref="ActivityGetResultRequest"/>
    /// </summary>
    [InternalProxyMessage(InternalMessageTypes.ActivityGetResultReply)]
    internal class ActivityGetResultReply : WorkflowReply
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ActivityGetResultReply()
        {
            Type = InternalMessageTypes.ActivityGetResultReply;
        }

        /// <summary>
        /// Returns the activity result encoded as bytes.
        /// </summary>
        public byte[] Result
        {
            get => GetBytesProperty(PropertyNames.Result);
            set => SetBytesProperty(PropertyNames.Result, value);
        }

        /// <inheritdoc/>
        internal override ProxyMessage Clone()
        {
            var clone = new ActivityGetResultReply();

            CopyTo(clone);

            return clone;
        }

        /// <inheritdoc/>
        protected override void CopyTo(ProxyMessage target)
        {
            base.CopyTo(target);

            var typedTarget = (ActivityGetResultReply)target;

            typedTarget.Result = this.Result;
        }
    }
}
