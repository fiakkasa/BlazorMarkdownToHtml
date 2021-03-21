using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;

namespace BlazorMarkdownToHtml.Pages
{
    public partial class Index : ComponentBase, IDisposable
    {
        internal record Input(string? Markdown = default, bool Format = false);

        private string markdown = string.Empty;
        private MarkupString html;
        private bool disposedValue;
        private readonly Regex normalizedExtraBits = new(@"(\[\]\:)");
        private IDisposable? inputSubscription;
        private readonly BehaviorSubject<Input> inputSubject = new(new(string.Empty));
        private bool formatDisabled = true;

        [Inject] private MarkdownPipeline? Pipeline { get; set; }

#pragma warning disable RCS1163 // Unused parameter.
        private void OnFormat(MouseEventArgs args)
        {
            if (formatDisabled) return;

            inputSubject.OnNext(new(markdown, true));
        }
#pragma warning restore RCS1163 // Unused parameter.

        private void OnInput(ChangeEventArgs e)
        {
            var input = new Input(e.Value?.ToString());
            ToggleFormatDisabled(input.Markdown);
            inputSubject.OnNext(input);
        }

        private void ToggleFormatDisabled(string? markdown)
        {
            formatDisabled = string.IsNullOrWhiteSpace(markdown);
            StateHasChanged();
        }

        private void ProcessInput(Input input)
        {
            markdown =
                input.Format
                    ? normalizedExtraBits.Replace(Markdown.Normalize(input.Markdown ?? string.Empty), "")
                    : input.Markdown ?? string.Empty;

            html = (MarkupString)Markdown.ToHtml(markdown, Pipeline);

            InvokeAsync(() => StateHasChanged());
        }

        protected override void OnInitialized()
        {
            inputSubscription =
                inputSubject
                    .Throttle(TimeSpan.FromMilliseconds(600))
                    .DistinctUntilChanged()
                    .Subscribe(ProcessInput);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) return;

            inputSubscription?.Dispose();
            disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
