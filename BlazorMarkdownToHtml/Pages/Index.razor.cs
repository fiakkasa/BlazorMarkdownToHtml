using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;

namespace BlazorMarkdownToHtml.Pages
{
    internal record Input(string? markdown = default, bool format = false);

    public partial class Index : ComponentBase, IDisposable
    {
        private string markdown = string.Empty;
        private MarkupString html;
        private bool disposedValue;
        private readonly Regex normalizedExtraBits = new(@"(\[\]\:)");
        private IDisposable? inputSubscription;
        private BehaviorSubject<Input> inputSubject = new(new(string.Empty));

        [Inject] private MarkdownPipeline? Pipeline { get; set; }

#pragma warning disable RCS1163 // Unused parameter.
        private void Format(MouseEventArgs args) =>
            inputSubject.OnNext(new(format: true));
#pragma warning restore RCS1163 // Unused parameter.

        private void OnInput(ChangeEventArgs e) =>
            inputSubject.OnNext(new(e.Value?.ToString()));

        private void ProcessInput(Input input)
        {
            if (input.format)
            {
                markdown = normalizedExtraBits.Replace(Markdown.Normalize(markdown), "");
            }
            else
            {
                markdown = input.markdown ?? string.Empty;
                html = (MarkupString)Markdown.ToHtml(markdown, Pipeline);
            }

            InvokeAsync(() => StateHasChanged());
        }

        protected override void OnInitialized()
        {
            inputSubscription =
                inputSubject
                    .Throttle(TimeSpan.FromMilliseconds(600))
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
