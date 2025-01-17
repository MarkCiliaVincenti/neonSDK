//-----------------------------------------------------------------------------
// FILE:        TransitionDemo.razor.cs
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
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using Neon.Tailwind;

namespace TestTailwind.Components
{
    public partial class TransitionDemo : ComponentBase
    {
        private bool            isShowing = true;
        private Transition      transition;
        private bool            isTransitioning = false;

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        private async Task ShowTransitionAsync()
        {
            if (isTransitioning) { return; }
            isTransitioning = true;

            await transition.LeaveAsync();

            await transition.EnterAsync();

            isTransitioning = false;
        }
    }
}
