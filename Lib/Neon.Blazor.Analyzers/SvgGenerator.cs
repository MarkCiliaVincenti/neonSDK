// -----------------------------------------------------------------------------
// FILE:	    SvgGenerator.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Neon.Blazor.Analyzers
{
    /// <summary>
    /// A generator to convert .svg files into Blazor components.
    /// </summary>
    [Generator]
    public class SvgGenerator : ISourceGenerator
    {
        private static List<string> attributesToKeep = new List<string>()
        {
            "viewBox"
        };

        /// <inheritdoc/>
        public void Execute(GeneratorExecutionContext context)
        {
            //System.Diagnostics.Debugger.Launch();

            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.NeonBlazorSvgClass", out var svgClass);
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.NeonBlazorSvgFill", out var svgFill);
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.NeonBlazorSvgStroke", out var svgStroke);
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.NeonBlazorSvgTargetNamespace", out var targetNamespace);

            if (string.IsNullOrEmpty(targetNamespace))
            {
                targetNamespace = "Neon.Blazor.Svg";
            }

            var svgFiles = context.AdditionalFiles
                .Where(file => file.Path.EndsWith(".svg"))
                .Select((file, _) => GetSvg(file));

            foreach (var svg in svgFiles)
            {
                var sb = new StringBuilder();

                sb.AppendLine(Constants.AutoGeneratedHeader);
                sb.AppendLine();

                var usings = new HashSet<string>()
                {
                    "System.Collections.Generic",
                    "System.Linq",
                    "Microsoft.AspNetCore.Components",
                    "Microsoft.AspNetCore.Components.Rendering",
                };

                var lastUsingRoot = "";
                foreach (var u in usings)
                {
                    var usingRoot = u.Split('.').First();

                    if (!string.IsNullOrEmpty(lastUsingRoot) && usingRoot != lastUsingRoot)
                    {
                        sb.AppendLine();
                    }

                    lastUsingRoot = usingRoot;

                    sb.AppendLine($"using {u};");
                }
                sb.AppendLine();

                sb.AppendLine($@"namespace {targetNamespace}
{{
    public partial class {svg.ClassName} : ComponentBase
    {{
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes {{ get; set; }}

        private Dictionary<string, object> Attributes {{ get; set; }}

        private string baseClass = ""{svgClass}"";
        private string fill = ""{svgFill}"";
        private string stroke = ""{svgStroke}"";

        protected override void OnParametersSet()
        {{
            Attributes = GetDefaultAttributes();

            if (AdditionalAttributes == null)
            {{
                StateHasChanged();

                return;
            }}

            AdditionalAttributes.TryGetValue(""class"", out var _class);

            var classes = baseClass.Split(' ').ToList();
            classes.AddRange(((string)_class).Split(' '));

            var svgClass = string.Join("" "", classes.ToHashSet());

            if (!string.IsNullOrEmpty(svgClass))
            {{
                Attributes[""class""] = svgClass;
            }}

            if (AdditionalAttributes.TryGetValue(""fill"", out var _fill))
            {{
                Attributes[""fill""] = _fill;
            }}
            else if (!string.IsNullOrEmpty(fill))
            {{
                Attributes[""fill""] = fill;
            }}

            if (AdditionalAttributes.TryGetValue(""stroke"", out var _stroke))
            {{
                Attributes[""stroke""] = _stroke;
            }}
            else if (!string.IsNullOrEmpty(stroke))
            {{
                Attributes[""stroke""] = stroke;
            }}

            StateHasChanged();

            base.OnParametersSet();
        }}

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {{
            builder.OpenElement(0, ""svg"");

            if (Attributes.Count > 0)
            {{
                builder.AddMultipleAttributes(1, Attributes.Select(a => new KeyValuePair<string, object>(a.Key, a.Value)));
            }}

            builder.AddMarkupContent(2, @""<path d=""""M4 5v11h16V5H4Zm-2-.993C2 3.451 2.455 3 2.992 3h18.016c.548 0 .992.449.992 1.007V18H2V4.007ZM1 19h22v2H1v-2Z"""" xmlns=""""http://www.w3.org/2000/svg"""" />"");
            builder.CloseElement();
        }}

        private static Dictionary<string, object> GetDefaultAttributes()
        {{
            var attributes = new Dictionary<string, object>();");

                foreach (var attr in svg.Attributes)
                {
                    sb.AppendLine($@"            attributes.Add(""{attr.Key}"", @""{attr.Value.Replace("\"", "\"\"")}"");");
                }
                sb.AppendLine($@"
            return attributes;
        }}
    }}
}}");

                var sourceString = sb.ToString();
                context.AddSource($"{svg.ClassName}.g.cs", sourceString);
            }

        }

        /// <inheritdoc/>
        public void Initialize(GeneratorInitializationContext context)
        {
            
        }

        /// <summary>
        /// Gets the class name fromt he svg file name.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetClassName(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);

            string className = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fileName);

            if (!SyntaxFacts.IsValidIdentifier(className))
            {
                // File name contains invalid chars, remove them
                Regex regex = new Regex(@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]");
                className = regex.Replace(className, "");

                // Class name doesn't begin with a letter, insert an underscore
                if (!char.IsLetter(className, 0))
                {
                    className = className.Insert(0, "_");
                }
            }

            return className.Replace(" ", string.Empty);
        }

        /// <summary>
        /// Creates an <see cref="Svg"/> instance.
        /// </summary>
        /// <param name="svgText"></param>
        /// <returns></returns>
        public static Svg GetSvg(AdditionalText svgText)
        {
            var xmlDoc = XDocument.Parse(svgText.GetText()?.ToString());
            var body   = string.Join("", xmlDoc.Root.Elements().Select(e => e.ToString(SaveOptions.DisableFormatting)));

            var svg = new Svg()
            {
                Attributes = new Dictionary<string, string>(),
                Body       = $@"@""{body.Replace("\"", "\"\"")}""",
                ClassName  = GetClassName(svgText.Path)
            };

            foreach (var attr in xmlDoc.Root.Attributes())
            {
                if (attributesToKeep.Contains(attr.Name.LocalName))
                {
                    svg.Attributes[attr.Name.LocalName] = attr.Value;
                }
            }

            return svg;
        }

        /// <summary>
        /// Contains info about an SVG file.
        /// </summary>
        public class Svg
        {
            /// <summary>
            /// The class name of the SVG
            /// </summary>
            public string ClassName { get; set; }

            /// <summary>
            /// The Body.
            /// </summary>
            public string Body { get; set; }

            /// <summary>
            /// Attributes that the original svg file has.
            /// </summary>
            public Dictionary<string, string> Attributes { get; set; }
        }
    }
}
