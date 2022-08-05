﻿//-----------------------------------------------------------------------------
// FILE:	    SyncSignalStatus.cs
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

using Newtonsoft.Json;

using Neon.Common;
using Neon.Tasks;
using Neon.Temporal;

namespace Neon.Temporal.Internal
{
    /// <summary>
    /// Holds the status of a synchronous signal execution.
    /// </summary>
    internal class SyncSignalStatus
    {
        /// <summary>
        /// Returns the dictionary of signal method arguments keyed by parameter name.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, object> Args { get; set; }

        /// <summary>
        /// Returns <c>true</c> if the workflow has finished executing the signal
        /// and that the result is available (for non-void signals).
        /// </summary>
        [JsonProperty(PropertyName = "Completed", Required = Required.Always)]
        public bool Completed { get; set; }

        /// <summary>
        /// <para>
        /// Returns potential error information when <see cref="Completed"/><c>=true</c>.  This
        /// will return <c>null</c> if the signal completed without error or else an error
        /// string describing the exception thrown by the signal method.
        /// </para>
        /// <note>
        /// This string must be formatted by <see cref="SyncSignalException.GetError(Exception)"/>.
        /// </note>
        /// </summary>
        [JsonProperty(PropertyName = "Error", Required = Required.AllowNull)]
        public string Error { get; set; }

        /// <summary>
        /// Returns the encoded result for signals that return results.  This will be <c>null</c> for 
        /// signals that don't return a result.
        /// </summary>
        [JsonProperty(PropertyName = "Result", Required = Required.AllowNull)]
        public byte[] Result { get; set; }

        /// <summary>
        /// Returns <c>true</c> after the workflow has returned the result of the
        /// completed signal operation to a polling query.  This is used internally
        /// to delay returning from the workflow while there remain outstanding
        /// synchronous signals that have not been answered.
        /// </summary>
        [JsonIgnore]
        public bool Acknowledged { get; set; }

        /// <summary>
        /// Returns the time when the signal was acknowledged.
        /// </summary>
        [JsonIgnore]
        public DateTime AcknowledgeTime { get; set; }
    }
}
