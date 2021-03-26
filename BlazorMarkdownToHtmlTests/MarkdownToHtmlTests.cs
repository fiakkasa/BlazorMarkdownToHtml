using Blazored.LocalStorage;
using Bunit;
using Markdig;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BlazorMarkdownToHtml.Components.Tests
{
    public class MarkdownToHtmlTests
    {
        [Fact(DisplayName = "Renders Correctly - Init")]
        public void MarkdownToHtmlRendersCorrectly()
        {
            using var ctx = new TestContext();

            ctx.Services.AddSingleton(new MarkdownPipelineBuilder().Build());
            ctx.Services.AddSingleton(new Mock<ILocalStorageService>().Object);

            var cut = ctx.RenderComponent<MarkdownToHtml>();

            var clearButton = cut.Find("button.btn-danger");
            var formatButton = cut.Find("button.btn-primary");

            cut.Find(".card-title > span").InnerHtml.MarkupMatches("Markdown to Html");
            clearButton.InnerHtml.MarkupMatches("Clear");
            Assert.NotNull(clearButton.GetAttribute("disabled"));
            formatButton.InnerHtml.MarkupMatches("Format");
            Assert.NotNull(formatButton.GetAttribute("disabled"));
            cut.Find("textarea.form-control").GetAttribute("value").MarkupMatches("");
            cut.Find("div.form-control").InnerHtml.MarkupMatches("");
        }
    }
}