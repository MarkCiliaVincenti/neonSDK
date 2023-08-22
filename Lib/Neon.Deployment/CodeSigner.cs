//-----------------------------------------------------------------------------
// FILE:        CodeSigner.cs
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
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Text;
using System.Threading;

using Neon.Common;
using Neon.IO;

namespace Neon.Deployment
{
    /// <summary>
    /// Implements code signing.
    /// </summary>
    public static class CodeSigner
    {
        /// <summary>
        /// Signs an EXE or MSI file using a code signing certificate and Microsoft Built Tools <b>signtool</b> program.
        /// </summary>
        /// <param name="targetPath">Specifies the path to the file being signed.</param>
        /// <param name="provider">Specifies the certificate provider, like: "eToken Base Cryptographic Provider"</param>
        /// <param name="certBase64">Specifies the base64 encoded public certificate (multi-line values are allowed).</param>
        /// <param name="container">Specifies the certificate container, like: "Sectigo_20220830143311"</param>
        /// <param name="timestampUri">Specifies the URI for the certificate timestamp service, like: http://timestamp.sectigo.com</param>
        /// <param name="password">Specifies the certificate password.</param>
        /// <exception cref="PlatformNotSupportedException">Thrown when executed on a non 64-bit Windows machine.</exception>
        /// <remarks>
        /// <note>
        /// <b>WARNING!</b> Be very careful when using this method with Extended Validation (EV) code signing 
        /// USB tokens.  Using an incorrect password can brick EV tokens since thay typically allow only a 
        /// very limited number of signing attempts with invalid passwords.
        /// </note>
        /// </remarks>
        public static void SignTool(
            string      targetPath, 
            string      provider, 
            string      certBase64, 
            string      container, 
            string      timestampUri,
            string      password)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(targetPath), nameof(targetPath));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(provider), nameof(provider));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(certBase64), nameof(certBase64));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(container), nameof(container));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(timestampUri), nameof(timestampUri));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(password), nameof(password));
            Covenant.Requires<PlatformNotSupportedException>(NeonHelper.IsWindows && NeonHelper.Is64BitOS, "This is supported only for 64-bit Windows.");

            // Strip out any CR/LFs from the certificate base64, convert to bytes 
            // and write to a temporary file so we'll be able to pass its path
            // to signtool.exe

            certBase64 = certBase64.Replace("\r", string.Empty);
            certBase64 = certBase64.Replace("\n", string.Empty);

            using (var tempFolder = new TempFolder())
            {
                var tempCertPath = Path.Combine(tempFolder.Path, "certificate.cer");
                var signToolPath = InstallSignTool(tempFolder.Path);

                File.WriteAllBytes(tempCertPath, Convert.FromBase64String(certBase64));

                NeonHelper.ExecuteCapture(signToolPath,
                    new object[]
                    {
                        "sign",
                        "/f", tempCertPath,
                        "/fd", "sha256",
                        "/tr", timestampUri,
                        "/td", "sha256",
                        "/csp", provider,
                        "/k", $"[{{{{{password}}}}}]={container}",
                        targetPath
                    })
                .EnsureSuccess();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider">Specifies the certificate provider, like: "eToken Base Cryptographic Provider"</param>
        /// <param name="certBase64">Specifies the base64 encoded public certificate (multi-line values are allowed).</param>
        /// <param name="container">Specifies the certificate container, like: "Sectigo_20220830143311"</param>
        /// <param name="timestampUri">Specifies the URI for the certificate timestamp service, like: http://timestamp.sectigo.com</param>
        /// <param name="password">Specifies the certificate password.</param>
        /// <returns><c>true</c> when signing is available.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown when executed on a non 64-bit Windows machine.</exception>
        /// <remarks>
        /// <note>
        /// <b>WARNING!</b> Be very careful when using this method with Extended Validation (EV) code signing 
        /// USB tokens.  Using an incorrect password can brick EV tokens since thay typically allow only a 
        /// very limited number of signing attempts with invalid passwords.
        /// </note>
        /// <note>
        /// This method uses the Windows version of <b>signtool.exe</b> embedded into the
        /// the <b>Neon.Deployment</b> library and to perform the code signing and this 
        /// tool runs only on Windows.
        /// </note>
        /// </remarks>
        public static bool IsReady(
            string      provider, 
            string      certBase64, 
            string      container, 
            string      timestampUri,
            string      password)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(provider), nameof(provider));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(certBase64), nameof(certBase64));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(container), nameof(container));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(timestampUri), nameof(timestampUri));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(password), nameof(password));
            Covenant.Requires<PlatformNotSupportedException>(NeonHelper.IsWindows && NeonHelper.Is64BitOS, "This is supported only for 64-bit Windows.");

            // We're going to verify that code signing can complete by signing
            // a copy of a small embedded executable.  This verifies that the parameters
            // are correct and also that the code-signing token is actually available.

            try
            {
                using (var tempFile = new TempFile(suffix: ".exe"))
                {
                    ExtractTestBinaryTo(tempFile.Path);

                    SignTool(
                        targetPath:   tempFile.Path,
                        provider:     provider,
                        certBase64:   certBase64,
                        container:    container,
                        timestampUri: timestampUri,
                        password:     password);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Extracts the <b>signtool.exe</b> binary from the embedded resource
        /// to the specified path.
        /// </summary>
        /// <param name="installFolder">The folder where the build tools will be installed.</param>
        /// <returns>The path to the SignTool binary.</returns>
        private static string InstallSignTool(string installFolder)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(installFolder));

            // We're going to use the nuget CLI to install the Microsoft Build Tools
            // within the folder specified which creates some subfolders.  The method
            // returns the path to the Windows x64 version of the SignTool binary.

            const string buildToolsVersion = "10.0.20348.19";

            NeonHelper.ExecuteCapture("nuget",
                new object[]
                {
                    "install",
                    "Microsoft.Windows.SDK.BuildTools",
                    "-Version", buildToolsVersion,
                    "-o", installFolder
                })
                .EnsureSuccess();

            var version       = Version.Parse(buildToolsVersion);
            var signToolPath  = Path.Combine(installFolder, $"Microsoft.Windows.SDK.BuildTools.{buildToolsVersion}", "bin", $"{version.Major}.{version.Minor}.{version.Build}.0", "x64", "signtool.exe");

            if (!File.Exists(signToolPath))
            {
                throw new FileNotFoundException($"SignTool installation signtool file does not found: {signToolPath}");
            }

            return signToolPath;
        }

        /// <summary>
        /// Extracts the <b>signee.exe</b> binary from the embedded resource
        /// to the specified path.
        /// </summary>
        /// <param name="targetPath">The target path for the binary.</param>
        private static void ExtractTestBinaryTo(string targetPath)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(targetPath));

            var assembly = Assembly.GetExecutingAssembly();

            using (var toolStream = assembly.GetManifestResourceStream("Neon.Deployment.Resources.Windows.signee.exe"))
            {
                using (var output = File.Create(targetPath))
                {
                    toolStream.CopyTo(output);
                }
            }
        }
    }
}
