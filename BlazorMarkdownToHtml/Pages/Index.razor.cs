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
        private string markdown = string.Empty;
        private MarkupString html;
        private bool disposedValue;
        private readonly Regex normalizedExtraBits = new(@"(\[\]\:)");
        private IDisposable? markdownToHtmlSubscription;
        private readonly BehaviorSubject<string> markdownToHtmlSubject = new(string.Empty);
        private IDisposable? formatMarkdownSubscription;
        private readonly BehaviorSubject<string> formatMarkdownSubject = new(string.Empty);
        private bool formatDisabled = true;

        [Inject] private MarkdownPipeline? Pipeline { get; set; }

#pragma warning disable RCS1163 // Unused parameter.
        private void OnFormat(MouseEventArgs args)
        {
            if (formatDisabled) return;

            formatDisabled = true;

            formatMarkdownSubject.OnNext(markdown);
        }
#pragma warning restore RCS1163 // Unused parameter.

        private void FormatMarkdown(string unformattedMarkdown)
        {
            markdown = normalizedExtraBits.Replace(Markdown.Normalize(unformattedMarkdown), "");

            formatDisabled = string.IsNullOrWhiteSpace(markdown);

            InvokeAsync(() => StateHasChanged());
        }

        private void OnInput(ChangeEventArgs e)
        {
            markdown = e.Value?.ToString() ?? string.Empty;

            formatDisabled = string.IsNullOrWhiteSpace(markdown);

            markdownToHtmlSubject.OnNext(markdown);
        }

        private void ProcessInput(string markdown)
        {
            html = (MarkupString)Markdown.ToHtml(markdown, Pipeline);

            InvokeAsync(() => StateHasChanged());
        }

        protected override void OnInitialized()
        {
            markdownToHtmlSubscription =
                markdownToHtmlSubject
                    .Throttle(TimeSpan.FromMilliseconds(600))
                    .DistinctUntilChanged()
                    .Subscribe(ProcessInput);

            formatMarkdownSubscription =
                formatMarkdownSubject
                    .Throttle(TimeSpan.FromMilliseconds(250))
                    .Subscribe(FormatMarkdown);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) return;

            markdownToHtmlSubscription?.Dispose();
            formatMarkdownSubscription?.Dispose();

            disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
