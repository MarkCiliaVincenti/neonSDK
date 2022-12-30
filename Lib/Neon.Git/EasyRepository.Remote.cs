﻿//-----------------------------------------------------------------------------
// FILE:	    EasyRepository.Remote.cs
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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Neon.Common;
using Neon.Deployment;

using LibGit2Sharp;
using LibGit2Sharp.Handlers;

using Octokit;

using GitHubBranch     = Octokit.Branch;
using GitHubRepository = Octokit.Repository;
using GitHubSignature  = Octokit.Signature;

using GitBranch     = LibGit2Sharp.Branch;
using GitRepository = LibGit2Sharp.Repository;
using GitSignature  = LibGit2Sharp.Signature;

namespace Neon.Git
{
    public partial class EasyRepository
    {
        /// <summary>
        /// Returns a specific remote branch.
        /// </summary>
        /// <param name="branchName">Specifies the remote branch name.</param>
        /// <returns>The <see cref="GitHubBranch"/> or <c>null</c> when the branch doesn't exist.</returns>
        public async Task<GitHubBranch> GetRemoteBranchAsync(string branchName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));

            return (await GetRemoteBranchesAsync()).FirstOrDefault(branch => branch.Name.Equals(branchName, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Returns all remote branches for a GitHub repo.
        /// </summary>
        /// <returns>The list of branches.</returns>
        public async Task<IReadOnlyList<GitHubBranch>> GetRemoteBranchesAsync()
        {
            return await RemoteApi.Repository.Branch.GetAll(RemoteRepoPath.Owner, RemoteRepoPath.Repo);
        }
    }
}
