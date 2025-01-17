//-----------------------------------------------------------------------------
// FILE:        LogAsTraceProcessorOptions.cs
// CONTRIBUTOR: Jeff Lill
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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Neon.Common;

using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;

namespace Neon.Diagnostics
{
    /// <summary>
    /// Specifies the options used to configure a <see cref="LogAsTraceProcessor"/>.
    /// </summary>
    public class LogAsTraceProcessorOptions
    {
        /// <summary>
        /// Constructs an instance with reasonable settings.
        /// </summary>
        public LogAsTraceProcessorOptions()
        {
        }

        /// <summary>
        /// <para>
        /// Used to filter the log events that are forwarded.  Only events with 
        /// log levels greater than or equal to this value will be also logged
        /// as trace events.
        /// </para>
        /// <para>
        /// This defaults to <see cref="LogLevel.Information"/>.
        /// </para>
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
    }
}
