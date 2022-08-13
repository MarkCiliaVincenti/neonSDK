﻿//-----------------------------------------------------------------------------
// FILE:	    LogLevel.cs
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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Neon.Diagnostics
{
    /// <summary>
    /// Enumerates the possible log levels.  Note that the relative
    /// ordinal values of  these definitions are used when deciding
    /// to log an event when a specific <see cref="LogLevel"/> is 
    /// set.  Only events with log levels less than or equal to the
    /// current level will be logged.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Logging is disabled.
        /// </summary>
        None = 0,

        /// <summary>
        /// A critical or fatal error has been detected.  These errors indicate that
        /// a very serious failure has occurred that may have crashed the program or
        /// at least seriousoly impacts its functioning.
        /// </summary>
        Critical = 100,

        /// <summary>
        /// A security related error has occurred.  Errors indicate a problem that may be
        /// transient, be recovered from, or are perhaps more serious.
        /// </summary>
        SError = 200,

        /// <summary>
        /// An error has been detected.  Errors indicate a problem that may be
        /// transient, be recovered from, or are perhaps more serious.
        /// </summary>
        Error = 300,

        /// <summary>
        /// An unusual condition has been detected that may ultimately lead to an error.
        /// </summary>
        Warn = 400,

        /// <summary>
        /// Describes a non-error security operation or condition, such as a 
        /// a successful login or authentication.
        /// </summary>
        SInfo = 500,

        /// <summary>
        /// Describes a normal operation or condition.
        /// </summary>
        Info = 600,

        /// <summary>
        /// Describes a transient error, typically logged by a <see cref="Neon.Retry.IRetryPolicy"/>
        /// implementations.
        /// </summary>
        Transient = 700,

        /// <summary>
        /// Describes detailed debug or diagnostic information.
        /// </summary>
        Debug = 800,

        /// <summary>
        /// Describes <b>very</b> detailed debug or diagnostic information.
        /// </summary>
        Trace = 900
    }
}
