﻿//-----------------------------------------------------------------------------
// FILE:	    ActivityStartLocalRequest.cs
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
    /// <b>client --> proxy:</b> Starts a local activity but doesn't wait for it to complete.
    /// </summary>
    [InternalProxyMessage(InternalMessageTypes.ActivityStartLocalRequest)]
    internal class ActivityStartLocalRequest : WorkflowRequest
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ActivityStartLocalRequest()
        {
            Type = InternalMessageTypes.ActivityStartLocalRequest;
        }

        /// <inheritdoc/>
        public override InternalMessageTypes ReplyType => InternalMessageTypes.ActivityStartLocalReply;

        /// <summary>
        /// Used to identify the activity.
        /// </summary>
        public long ActivityId
        {
            get => GetLongProperty(PropertyNames.ActivityId);
            set => SetLongProperty(PropertyNames.ActivityId, value);
        }

        /// <summary>
        /// Identifies the .NET type that implements the local activity. 
        /// </summary>
        public long ActivityTypeId
        {
            get => GetLongProperty(PropertyNames.ActivityTypeId);
            set => SetLongProperty(PropertyNames.ActivityTypeId, value);
        }

        /// <summary>
        /// Optionally specifies the arguments to be passed to the activity encoded
        /// as a byte array.
        /// </summary>
        public byte[] Args
        {
            get => GetBytesProperty(PropertyNames.Args);
            set => SetBytesProperty(PropertyNames.Args, value);
        }

        /// <summary>
        /// The local activity options.
        /// </summary>
        public InternalLocalActivityOptions Options
        {
            get => GetJsonProperty<InternalLocalActivityOptions>(PropertyNames.Options);
            set => SetJsonProperty<InternalLocalActivityOptions>(PropertyNames.Options, value);
        }

        /// <inheritdoc/>
        internal override ProxyMessage Clone()
        {
            var clone = new ActivityStartLocalRequest();

            CopyTo(clone);

            return clone;
        }

        /// <inheritdoc/>
        protected override void CopyTo(ProxyMessage target)
        {
            base.CopyTo(target);

            var typedTarget = (ActivityStartLocalRequest)target;

            typedTarget.ActivityId     = this.ActivityId;
            typedTarget.ActivityTypeId = this.ActivityTypeId;
            typedTarget.Args           = this.Args;
            typedTarget.Options        = this.Options;
        }
    }
}
