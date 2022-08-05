﻿//-----------------------------------------------------------------------------
// FILE:	    ActivityRegisterRequest.cs
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
    /// <b>client --> proxy:</b> Registers a workflow handler by name.
    /// </summary>
    [InternalProxyMessage(InternalMessageTypes.ActivityRegisterRequest)]
    internal class ActivityRegisterRequest : ActivityRequest
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ActivityRegisterRequest()
        {
            Type = InternalMessageTypes.ActivityRegisterRequest;
        }

        /// <inheritdoc/>
        public override InternalMessageTypes ReplyType => InternalMessageTypes.ActivityRegisterReply;

        /// <summary>
        /// Identifies the activity implementation (AKA the activity type name).
        /// </summary>
        public string Name
        {
            get => GetStringProperty(PropertyNames.Name);
            set => SetStringProperty(PropertyNames.Name, value);
        }

        /// <summary>
        /// Disables checks for duplicate registrations.
        /// </summary>
        public bool DisableAlreadyRegisteredCheck
        {
            get => GetBoolProperty(PropertyNames.DisableAlreadyRegisteredCheck);
            set => SetBoolProperty(PropertyNames.DisableAlreadyRegisteredCheck, value);
        }

        /// <inheritdoc/>
        internal override ProxyMessage Clone()
        {
            var clone = new ActivityRegisterRequest();

            CopyTo(clone);

            return clone;
        }

        /// <inheritdoc/>
        protected override void CopyTo(ProxyMessage target)
        {
            base.CopyTo(target);

            var typedTarget = (ActivityRegisterRequest)target;

            typedTarget.Name                          = this.Name;
            typedTarget.DisableAlreadyRegisteredCheck = this.DisableAlreadyRegisteredCheck;
        }
    }
}
