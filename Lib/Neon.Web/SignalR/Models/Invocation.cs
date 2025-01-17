//-----------------------------------------------------------------------------
// FILE:        Invocation.cs
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR.Protocol;

using Neon.Common;

using MessagePack;

using Newtonsoft.Json;

namespace Neon.Web.SignalR
{
    /// <summary>
    /// Represents a method invokation.
    /// </summary>
    [MessagePackObject]
    public class Invocation
    {
        /// <summary>
        /// The optional invokation ID.
        /// </summary>
        [Key(0)]
        public string InvocationId { get; set; } = null;

        /// <summary>
        /// The method name.
        /// </summary>
        [Key(1)]
        public string MethodName { get; set; } = null;

        /// <summary>
        /// The method arguments.
        /// </summary>
        [Key(2)]
        public object[] Args { get; set; } = null;

        /// <summary>
        /// The list of connection IDs that should not receive this message.
        /// </summary>
        [Key(3)]
        public IReadOnlyList<string> ExcludedConnectionIds { get; set; } = null;

        /// <summary>
        /// The optional return channel.
        /// </summary>
        [Key(5)]
        public string ReturnChannel { get; set; } = null;

        /// <summary>
        /// Writes a <see cref="Invocation"/> to a byte array.
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static byte[] Write(string methodName, object[] args) => Write(methodName: methodName, args: args, excludedConnectionIds: null);

        /// <summary>
        /// Writes a <see cref="Invocation"/> to a byte array.
        /// </summary>
        /// <param name="methodName">The method name.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="invocationId">Optionally specifies an invocation ID.</param>
        /// <param name="excludedConnectionIds">Optionally specifies excluded connection IDs.</param>
        /// <param name="returnChannel">Optionally specifies the return channel.</param>
        /// <returns>The serialized message.</returns>
        public static byte[] Write(string methodName, object[] args, string invocationId = null, IReadOnlyList<string> excludedConnectionIds = null, string returnChannel = null)
        {
            var invokation = new Invocation()
            {
                InvocationId          = invocationId,
                MethodName            = methodName,
                Args                  = args,
                ExcludedConnectionIds = excludedConnectionIds,
                ReturnChannel         = returnChannel
            };

            return MessagePackSerializer.Serialize(invokation);
        }

        /// <summary>
        /// Reads an <see cref="Invocation"/> from a byte array.
        /// </summary>
        /// <param name="message">The message bytes.</param>
        /// <returns>The parsed <see cref="Invocation"/>.</returns>
        public static Invocation Read(byte[] message)
        {
            return MessagePackSerializer.Deserialize<Invocation>(message);
        }
    }
}
