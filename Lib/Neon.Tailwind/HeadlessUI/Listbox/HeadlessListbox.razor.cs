//-----------------------------------------------------------------------------
// FILE:        HeadlessListbox.razor.cs
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

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Neon.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neon.Tailwind
{
    public partial class HeadlessListbox<TValue> : IDisposable
    {
        [Parameter] public RenderFragment<HeadlessListbox<TValue>> ChildContent { get; set; }

        [Parameter] public TValue Value { get; set; }
        [Parameter] public EventCallback<TValue> ValueChanged { get; set; }

        [Parameter] public EventCallback OnOpen { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        [Parameter] public int DebouceTimeout { get; set; } = 350;

        public TValue CurrentValue
        {
            get => Value;
            set
            {
                bool valueChanged = !EqualityComparer<TValue>.Default.Equals(Value, value);
                if (valueChanged)
                {
                    Value = value;
                    ValueChanged.InvokeAsync(value);
                }
            }
        }

        private readonly List<HeadlessListboxOption<TValue>> options = new();
        private HeadlessListboxOption<TValue> activeOption;
        private ClickOffEventHandler clickOffEventHandler;
        private SearchAssistant searchAssistant;

        private HeadlessListboxButton<TValue> buttonElement;
        private HeadlessListboxOptions<TValue> optionsElement;
        private HeadlessListboxLabel<TValue> labelElement;

        public ListboxState State { get; protected set; } = ListboxState.Closed;
        public string SearchQuery => searchAssistant.SearchQuery;

        public string LabelId => labelElement?.Id;
        public string ActiveOptionId => activeOption?.Id;
        public string ButtonElementId => buttonElement?.Id;
        public string OptionsElementId => optionsElement?.Id;

        /// <summary>
        /// Con
        /// </summary>
        public HeadlessListbox()
        {
            searchAssistant = new SearchAssistant();
        }

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            searchAssistant.OnChange += HandleSearchChange!;
            base.OnInitialized();
        }

        /// <inheritdoc/>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await SyncContext.Clear;
            
            if (shouldFocus)
            {
                shouldFocus = false;
                if (State == ListboxState.Open)
                {
                    await OptionsFocusAsync();
                }
                else
                {
                    await ButtonFocusAsync();
                }
            }

            if (clickOffEventHandler != null)
            {
                await clickOffEventHandler.RegisterElement(buttonElement);
                await clickOffEventHandler.RegisterElement(optionsElement);
            }
        }

        public async Task SetValue(TValue value)
        {
            bool valueChanged = !EqualityComparer<TValue>.Default.Equals(Value, value);
            Value = value;
            if (valueChanged)
                await ValueChanged.InvokeAsync(value);
            await Close();
        }

        public void RegisterOption(HeadlessListboxOption<TValue> option)
        {
            options.Add(option);
        }
        public void UnregisterOption(HeadlessListboxOption<TValue> option)
        {
            if (!options.Contains(option)) return;

            if (activeOption == option)
            {
                GoToOption(ListboxFocus.Next);
            }
            options.Remove(option);
        }
        public bool IsActiveOption(HeadlessListboxOption<TValue> option) => activeOption == option;
        public void GoToOption(HeadlessListboxOption<TValue> option)
        {
            if (option != null && (!option.IsEnabled || !options.Contains(option))) option = null;
            if (activeOption == option) return;

            activeOption = option;
            StateHasChanged();
        }
        public void GoToOption(ListboxFocus focus)
        {
            switch (focus)
            {
                case ListboxFocus.First:
                    {
                        GoToOption(options.FirstOrDefault(mi => mi.IsEnabled));
                        break;
                    }
                case ListboxFocus.Previous:
                    {
                        GoToOption(FindOptionBeforeActiveOption());
                        break;
                    }
                case ListboxFocus.Next:
                    {
                        GoToOption(FindOptionAfterActiveOption());
                        break;
                    }
                case ListboxFocus.Last:
                    {
                        activeOption = options.LastOrDefault(mi => mi.IsEnabled);
                        break;
                    }
                default:
                    {
                        GoToOption(null);
                        break;
                    }
            }
        }
        private HeadlessListboxOption<TValue> FindOptionBeforeActiveOption()
        {
            var reversedMenuOptions = options.ToList();
            reversedMenuOptions.Reverse();
            bool foundTarget = false;
            var itemIndex = reversedMenuOptions.FindIndex(0, mi =>
            {
                if (mi == activeOption)
                {
                    foundTarget = true;
                    return false;
                }
                return foundTarget && mi.IsEnabled;
            });
            if (itemIndex != -1)
                return reversedMenuOptions[itemIndex];
            else
                return options.LastOrDefault(mi => mi.IsEnabled);
        }
        private HeadlessListboxOption<TValue> FindOptionAfterActiveOption()
        {
            bool foundTarget = false;
            var itemIndex = options.FindIndex(0, mi =>
            {
                if (mi == activeOption)
                {
                    foundTarget = true;
                    return false;
                }
                return foundTarget && mi.IsEnabled;
            });
            if (itemIndex != -1)
                return options[itemIndex];
            else
                return options.FirstOrDefault(mi => mi.IsEnabled);
        }

        public void RegisterButton(HeadlessListboxButton<TValue> button)
            => buttonElement = button;
        public void RegisterOptions(HeadlessListboxOptions<TValue> options)
            => optionsElement = options;
        public void Registerlabel(HeadlessListboxLabel<TValue> label)
            => labelElement = label;

        private bool shouldFocus;
        public async Task Toggle()
        {
            if (State == ListboxState.Closed)
                await Open();
            else
                await Close();
        }
        public async Task Close(bool suppressFocus = false)
        {
            if (State == ListboxState.Closed) return;
            State = ListboxState.Closed;
            await OnClose.InvokeAsync();
            activeOption = null;
            shouldFocus = !suppressFocus;
            StateHasChanged();
        }
        public async Task Open()
        {
            if (State == ListboxState.Open) return;
            State = ListboxState.Open;
            await OnOpen.InvokeAsync();
            shouldFocus = true;
            StateHasChanged();
        }
        public ValueTask OptionsFocusAsync() => optionsElement?.FocusAsync() ?? ValueTask.CompletedTask;
        public ValueTask ButtonFocusAsync() => buttonElement?.FocusAsync() ?? ValueTask.CompletedTask;
        public void SetActiveAsValue() => CurrentValue = activeOption is null ? default : activeOption.Value;

        public Task HandleClickOff() => Close();
        private void HandleSearchChange(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(SearchQuery))
            {
                var item = options.FirstOrDefault(mi => (mi.SearchValue ?? "").StartsWith(SearchQuery, StringComparison.OrdinalIgnoreCase) && mi.IsEnabled);
                GoToOption(item);
            }
        }
        public async Task SearchAsync(string key)
        {
            await searchAssistant.SearchAsync(key);
        }


        public void Dispose() => searchAssistant.Dispose();
    }
}
