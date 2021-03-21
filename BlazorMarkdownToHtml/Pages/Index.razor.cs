using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Text.RegularExpressions;

namespace BlazorMarkdownToHtml.Pages
{
    public partial class Index : ComponentBase
    {
        private string input = string.Empty;
        private MarkupString html;
        private readonly Regex normalizedExtraBits = new(@"(\[\]\:)");

        [Inject] private MarkdownPipeline? Pipeline { get; set; }

#pragma warning disable RCS1163 // Unused parameter.
        private void Format(MouseEventArgs args)
#pragma warning restore RCS1163 // Unused parameter.
        {
            input = normalizedExtraBits.Replace(Markdown.Normalize(input), "");

            StateHasChanged();
        }

        private void ProcessInput(ChangeEventArgs e)
        {
            input = (e.Value?.ToString() ?? string.Empty);
            html = (MarkupString)Markdown.ToHtml(input, Pipeline);

            StateHasChanged();
        }
    }
}
