﻿//-----------------------------------------------------------------------------
// FILE:	    StubCompilerException.cs
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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Neon.Cadence
{
    /// <summary>
    /// Thrown when there's a problem compiling a workflow or activity stub.
    /// </summary>
    public class StubCompilerException : Exception
    {
        //---------------------------------------------------------------------
        // Static members

        /// <summary>
        /// Converts compiler diagnostics into a string.
        /// </summary>
        /// <param name="diagnostics">The compiler diagnostics.</param>
        /// <param name="source">Optionally specifies the invalid source code.</param>
        /// <returns>The message string.</returns>
        private static string GetMessage(IEnumerable<Diagnostic> diagnostics, string source = null)
        {
            Covenant.Requires<ArgumentNullException>(diagnostics != null, nameof(diagnostics));
            Covenant.Requires<ArgumentException>(diagnostics.Count() > 0, nameof(diagnostics));

            var sb = new StringBuilder();

            foreach (var diagnostic in diagnostics)
            {
                sb.AppendLine(diagnostic.ToString());
            }

            if (!string.IsNullOrEmpty(source))
            {
                sb.AppendLine();
                sb.AppendLine("SOURCE:");
                sb.AppendLine("-------------------------------------------------------------------------------");
                sb.Append(source);
            }

            return sb.ToString();
        }

        //---------------------------------------------------------------------
        // Instance members

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="diagnostics">The compiler diagnostics.</param>
        /// <param name="source">Optionally specifies the invalid source code.</param>
        public StubCompilerException(IEnumerable<Diagnostic> diagnostics, string source = null)
            : base(GetMessage(diagnostics, source))
        {
        }
    }
}
