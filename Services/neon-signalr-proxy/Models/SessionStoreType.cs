//-----------------------------------------------------------------------------
// FILE:        SessionStoreType.cs
// CONTRIBUTOR: Marcus Bowyer
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
using System.Runtime.Serialization;

namespace NeonSignalRProxy
{
    /// <summary>
    /// Enumerates the possible cache backend types.
    /// </summary>
    public enum SessionStoreType
    {
        /// <summary>
        /// Session information is stored in a cookie.
        /// </summary>
        [EnumMember(Value = "cookie")]
        Cookie,

        /// <summary>
        /// Stored in the cache.
        /// </summary>
        [EnumMember(Value = "cache")]
        Cache
    }
}
