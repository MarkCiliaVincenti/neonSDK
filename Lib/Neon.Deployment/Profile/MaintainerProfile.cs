﻿//-----------------------------------------------------------------------------
// FILE:	    MaintainerProfile.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Neon.Common;

namespace Neon.Deployment
{
    /// <summary>
    /// Provides the <see cref="IProfileClient"/> implementation used by NEONFORGE maintainers
    /// to obtain 1Password secrets via our internal <b>neon-assistant</b> tool.
    /// </summary>
    /// <remarks>
    /// <note>
    /// This class uses process environment variables like <b>NEONASSISTANT_CACHE_PROFILE_[name=value</b>
    /// and <b>NEONASSISTANT_CACHE_SECRET_name=value</b> to cache profile and secret values such that
    /// other <see cref="MaintainerProfile"/> instances within the current process and
    /// subprocesses can take advantage of cached values as well.
    /// </note>
    /// </remarks>
    public partial class MaintainerProfile : IProfileClient
    {
        //---------------------------------------------------------------------
        // Local types

        internal enum CachingMode
        {
            /// <summary>
            /// Attempt to retrieve values from both the memory and environment
            /// variable caches in addition to the actual source.
            /// </summary>
            Normal = 0,

            /// <summary>
            /// Used for unit testing: Only attempt to retrieve values from the
            /// environment variable cache and not the source.
            /// </summary>
            EnvironmentOnly,

            /// <summary>
            /// Used for unit testing: Only attempt to retrieve values from the
            /// memory cache and not the source.
            /// </summary>
            MemoryOnly
        }

        //---------------------------------------------------------------------
        // Static members

        /// <summary>
        /// Variable name prefix used for caching secrets and profile values as environment variables.
        /// </summary>
        internal const string EnvironmentCachePrefix = "NEONASSISTANT_CACHE_";

        /// <summary>
        /// Variable name prefix used for caching profile values as environment variables.
        /// </summary>
        internal const string ProfileEnvironmentCachePrefix = $"{EnvironmentCachePrefix}PROFILE_";

        /// <summary>
        /// Variable name prefix used for caching secret values as environment variables.
        /// </summary>
        internal const string SecretEnvironmentCachePrefix = $"{EnvironmentCachePrefix}SECRET_";

        /// <summary>
        /// Clears any secrets or profile values cached as environment variables.
        /// </summary>
        internal static void ClearEnvironmentCache()
        {
            foreach (DictionaryEntry variable in Environment.GetEnvironmentVariables())
            {
                var name = (string)variable.Key;

                if (name.StartsWith(EnvironmentCachePrefix, StringComparison.InvariantCultureIgnoreCase))
                {
                    Environment.SetEnvironmentVariable(name, null);
                }
            }
        }

        //---------------------------------------------------------------------
        // Instance members

        private readonly string             pipeName;
        private readonly TimeSpan           connectTimeout;
        private bool                        cacheEnabled            = true;
        private CachingMode                 cacheMode               = CachingMode.Normal;
        private object                      syncLock                = new object();
        private Dictionary<string, string>  profileCache            = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<string, string>  secretCache             = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// <para>
        /// Constructs a profile client with default parameters.  This is suitable for 
        /// constructing from Powershell scripts.
        /// </para>
        /// <note>
        /// <see cref="MaintainerProfile"/> currently only supports Windows.
        /// </note>
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when not running on Windows.</exception>
        public MaintainerProfile()
            : this(DeploymentHelper.NeonProfileServicePipe)
        {
        }

        /// <summary>
        /// <para>
        /// Constructor with optional client timeout.
        /// </para>
        /// <note>
        /// <see cref="MaintainerProfile"/> currently supports only Windows.
        /// </note>
        /// </summary>
        /// <param name="pipeName">Specifies the server pipe name.</param>
        /// <param name="connectTimeout">Optionally specifies the connection timeout.  This defaults to <b>10 seconds</b>.</param>
        /// <exception cref="NotSupportedException">Thrown when not running on Windows.</exception>
        public MaintainerProfile(string pipeName, TimeSpan connectTimeout = default)
        {
            Covenant.Requires<NotSupportedException>(NeonHelper.IsWindows, $"[{nameof(MaintainerProfile)}] currently only supports Windows.");
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(pipeName), nameof(pipeName));

            this.pipeName = pipeName;

            if (connectTimeout <= TimeSpan.Zero)
            {
                connectTimeout = TimeSpan.FromSeconds(10);
            }

            this.connectTimeout = connectTimeout;
        }

        /// <inheritdoc/>
        public bool CacheEnabled
        {
            get => cacheEnabled;

            set
            {
                if (!value)
                {
                    CacheMode = CachingMode.Normal;

                    ClearCache();
                }

                cacheEnabled = value;
            }
        }

        internal CachingMode CacheMode
        {
            get => cacheMode;

            set
            {
                cacheMode = value;

                if (cacheMode != CachingMode.Normal)
                {
                    cacheEnabled = true;
                }
            }
        }

        /// <summary>
        /// Submits a request to the profile server and returns the response.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The response.</returns>
        /// <exception cref="ProfileException">Thrown if the profile server returns an error.</exception>
        private IProfileResponse Call(IProfileRequest request)
        {
            Covenant.Requires<ArgumentNullException>(request != null, nameof(request));

            using (var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut))
            {
                try
                {
                    pipe.Connect((int)connectTimeout.TotalMilliseconds);
                }
                catch (TimeoutException e)
                {
                    throw new ProfileException("Cannot connect to profile server.  Is [neon-assistant] running?", ProfileStatus.Timeout, e);
                }

                var reader = new StreamReader(pipe);
                var writer = new StreamWriter(pipe);

                writer.AutoFlush = true;
                writer.WriteLine(request);

                var responseLine = reader.ReadLine();

                if (responseLine == null)
                {
                    throw new ProfileException("The profile server did not respond.", ProfileStatus.Connect);
                }

                var response = ProfileResponse.Parse(responseLine);

                pipe.Close();

                if (!response.Success)
                {
                    throw new ProfileException(response.Error, response.Status);
                }

                return response;
            }
        }

        /// <inheritdoc/>
        public string GetProfileValue(string name, bool nullOnNotFound = false)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name), nameof(name));

            string value;

            if (CacheEnabled)
            {
                lock (syncLock)
                {
                    switch (cacheMode)
                    {
                        case CachingMode.Normal:

                            if (profileCache.TryGetValue(name, out value))
                            {
                                return value;
                            }

                            value = Environment.GetEnvironmentVariable($"{ProfileEnvironmentCachePrefix}{name}");

                            if (!string.IsNullOrEmpty(value))
                            {
                                return value;
                            }
                            break;

                        case CachingMode.EnvironmentOnly:

                            value = Environment.GetEnvironmentVariable($"{ProfileEnvironmentCachePrefix}{name}");

                            if (!string.IsNullOrEmpty(value))
                            {
                                return value;
                            }
                            break;

                        case CachingMode.MemoryOnly:

                            if (profileCache.TryGetValue(name, out value))
                            {
                                return value;
                            }
                            break;

                        default:

                            throw new NotImplementedException();
                    }
                }
            }

            if (cacheMode != CachingMode.Normal)
            {
                if (nullOnNotFound)
                {
                    return null;
                }
                else
                {
                    throw new ProfileException($"[{name}] profile value not found in the cache.", ProfileStatus.NotFound);
                }
            }

            var args = new Dictionary<string, string>();

            args.Add("name", name);

            try
            {
                value = Call(ProfileRequest.Create("GET-PROFILE-VALUE", args)).Value;

                if (cacheEnabled)
                {
                    lock (syncLock)
                    {
                        profileCache[name] = value;

                        Environment.SetEnvironmentVariable($"{ProfileEnvironmentCachePrefix}{name}", value);
                    }
                }

                return value;
            }
            catch (ProfileException e)
            {
                if (nullOnNotFound && e.Status == ProfileStatus.NotFound)
                {
                    return null;
                }

                throw;
            }
        }

        /// <inheritdoc/>
        public string GetSecretPassword(string name, string vault = null, string masterPassword = null, bool nullOnNotFound = false)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name), nameof(name));

            var     cacheKey = $"{vault}::{name}[password]";
            string  value;

            if (CacheEnabled)
            {
                lock (syncLock)
                {
                    switch (cacheMode)
                    {
                        case CachingMode.Normal:

                            if (secretCache.TryGetValue(name, out value))
                            {
                                return value;
                            }

                            value = Environment.GetEnvironmentVariable($"{SecretEnvironmentCachePrefix}{cacheKey}");

                            if (!string.IsNullOrEmpty(value))
                            {
                                return value;
                            }
                            break;

                        case CachingMode.EnvironmentOnly:

                            value = Environment.GetEnvironmentVariable($"{SecretEnvironmentCachePrefix}{cacheKey}");

                            if (!string.IsNullOrEmpty(value))
                            {
                                return value;
                            }
                            break;

                        case CachingMode.MemoryOnly:

                            if (secretCache.TryGetValue(name, out value))
                            {
                                return value;
                            }
                            break;

                        default:

                            throw new NotImplementedException();
                    }
                }
            }

            if (cacheMode != CachingMode.Normal)
            {
                if (nullOnNotFound)
                {
                    return null;
                }
                else
                {
                    throw new ProfileException($"[{name}] secret value not found in the cache.", ProfileStatus.NotFound);
                }
            }

            var args = new Dictionary<string, string>();

            args.Add("name", name);

            if (!string.IsNullOrEmpty(vault))
            {
                args.Add("vault", vault);
            }

            if (!string.IsNullOrEmpty(masterPassword))
            {
                args.Add("masterpassword", masterPassword);
            }

            try
            {
                value = Call(ProfileRequest.Create("GET-SECRET-PASSWORD", args)).Value;

                if (CacheEnabled)
                {
                    lock (syncLock)
                    {
                        secretCache[cacheKey] = value;

                        Environment.SetEnvironmentVariable($"{SecretEnvironmentCachePrefix}{cacheKey}", value);
                    }
                }

                return value;
            }
            catch (ProfileException e)
            {
                if (nullOnNotFound && e.Status == ProfileStatus.NotFound)
                {
                    return null;
                }

                throw;
            }
        }

        /// <inheritdoc/>
        public string GetSecretValue(string name, string vault = null, string masterPassword = null, bool nullOnNotFound = false)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name), nameof(name));

            var     cacheKey = $"{vault}::{name}[password]";
            string  value;

            if (CacheEnabled)
            {
                lock (syncLock)
                {
                    switch (cacheMode)
                    {
                        case CachingMode.Normal:

                            if (secretCache.TryGetValue(name, out value))
                            {
                                return value;
                            }

                            value = Environment.GetEnvironmentVariable($"{SecretEnvironmentCachePrefix}{cacheKey}");

                            if (!string.IsNullOrEmpty(value))
                            {
                                return value;
                            }
                            break;

                        case CachingMode.EnvironmentOnly:

                            value = Environment.GetEnvironmentVariable($"{SecretEnvironmentCachePrefix}{cacheKey}");

                            if (!string.IsNullOrEmpty(value))
                            {
                                return value;
                            }
                            break;

                        case CachingMode.MemoryOnly:

                            if (secretCache.TryGetValue(cacheKey, out value))
                            {
                                return value;
                            }
                            break;

                        default:

                            throw new NotImplementedException();
                    }
                }
            }

            if (cacheMode != CachingMode.Normal)
            {
                if (nullOnNotFound)
                {
                    return null;
                }
                else
                {
                    throw new ProfileException($"[{name}] secret value not found in the cache.", ProfileStatus.NotFound);
                }
            }

            var args = new Dictionary<string, string>();

            args.Add("name", name);

            if (!string.IsNullOrEmpty(vault))
            {
                args.Add("vault", vault);
            }

            if (!string.IsNullOrEmpty(masterPassword))
            {
                args.Add("masterpassword", masterPassword);
            }

            try
            {
                value = Call(ProfileRequest.Create("GET-SECRET-VALUE", args)).Value;

                if (CacheEnabled)
                {
                    lock (syncLock)
                    {
                        secretCache[cacheKey] = value;

                        Environment.SetEnvironmentVariable($"{SecretEnvironmentCachePrefix}{cacheKey}", value);
                    }
                }

                return value;
            }
            catch (ProfileException e)
            {
                if (nullOnNotFound && e.Status == ProfileStatus.NotFound)
                {
                    return null;
                }

                throw;
            }
        }

        /// <summary>
        /// Proactively used to retrieve secrets commonly used for CI/CD operations so they'll
        /// be available even after <b>neon-assistant</b> has signed out some time later.  
        /// Currently, AWS and GitHub credentials are retrieved.
        /// </summary>
        public void GetCommonSecrets()
        {
            GetAwsCredentials();
            GitHub.GetCredentials();
        }

        /// <inheritdoc/>
        public void ClearCache()
        {
            lock (syncLock)
            {
                profileCache.Clear();
                secretCache.Clear();
                ClearEnvironmentCache();
            }
        }

        /// <inheritdoc/>
        public string Call(Dictionary<string, string> args)
        {
            Covenant.Requires<ArgumentNullException>(args != null, nameof(args));

            return Call(ProfileRequest.Create("CALL", args)).Value;
        }
    }
}
