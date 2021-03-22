using Blazored.LocalStorage;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BlazorMarkdownToHtml.Components
{
    public partial class MarkdownToHtml : ComponentBase, IDisposable
    {
        private MarkupString html;
        private string markdown = string.Empty;
        private readonly Regex normalizedExtraBits = new(@"(\[\]\:)");
        private bool actionsDisabled = true;
        private IDisposable? formatMarkdownSubscription;
        private readonly BehaviorSubject<long> formatMarkdownSubject = new(0);
        private IDisposable? inputMarkdownSubscription;
        private readonly BehaviorSubject<string> inputMarkdownSubject = new(string.Empty);
        private const string storageKey = "BlazorMarkdownToHtml";
        private bool persist;
        private bool disposedValue;

        [Inject] private MarkdownPipeline? Pipeline { get; set; }
        [Inject] private ILocalStorageService? LocalStorage { get; set; }

#pragma warning disable RCS1163 // Unused parameter.
        private void OnFormat(MouseEventArgs args)
        {
            if (actionsDisabled) return;

            actionsDisabled = true;

            formatMarkdownSubject.OnNext(DateTime.Now.Ticks);
        }
#pragma warning restore RCS1163 // Unused parameter.

#pragma warning disable RCS1163 // Unused parameter.
        private void OnClear(MouseEventArgs args)
        {
            if (actionsDisabled) return;

            persist = true;
            SetUIVariables(string.Empty);
            html = (MarkupString)Markdown.ToHtml(markdown, Pipeline);
        }
#pragma warning restore RCS1163 // Unused parameter.

        private void OnInput(ChangeEventArgs e) =>
            inputMarkdownSubject.OnNext(e.Value?.ToString() ?? string.Empty);

        private void SetUIVariables(string? markdownValue)
        {
            markdown = string.IsNullOrWhiteSpace(markdownValue) ? string.Empty : markdownValue;
            actionsDisabled = markdown.Length == 0;
        }

        protected override void OnInitialized()
        {
            formatMarkdownSubscription =
                formatMarkdownSubject
                    .Skip(1)
                    .Select(_ => markdown)
                    .Throttle(TimeSpan.FromMilliseconds(250))
                    .Select(markdownValue => normalizedExtraBits.Replace(Markdown.Normalize(markdownValue), ""))
                    .Do(markdownValue =>
                    {
                        persist = markdown != markdownValue;
                        SetUIVariables(markdownValue);
                        InvokeAsync(() => StateHasChanged());
                    })
                    .Subscribe();

            inputMarkdownSubscription =
                inputMarkdownSubject
                    .Skip(1)
                    .Throttle(TimeSpan.FromMilliseconds(600))
                    .Do(markdownValue =>
                    {
                        persist = markdown != markdownValue;
                        SetUIVariables(markdownValue);
                        html = (MarkupString)Markdown.ToHtml(markdown, Pipeline);
                        InvokeAsync(() => StateHasChanged());
                    })
                    .Subscribe();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                SetUIVariables(await LocalStorage!.GetItemAsync<string?>(storageKey).ConfigureAwait(true));

                if (markdown.Length > 0)
                {
                    StateHasChanged();
                    inputMarkdownSubject.OnNext(markdown);
                }
            }
            else if (persist)
            {
                await LocalStorage!.SetItemAsync(storageKey, markdown).ConfigureAwait(true);
                persist = false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) return;

            formatMarkdownSubscription?.Dispose();
            inputMarkdownSubscription?.Dispose();

            disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

