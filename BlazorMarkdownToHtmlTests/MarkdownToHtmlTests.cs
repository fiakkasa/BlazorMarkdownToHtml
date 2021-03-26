using Blazored.LocalStorage;
using Bunit;
using Markdig;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Xunit;

namespace BlazorMarkdownToHtml.Components.Tests
{
    public class MarkdownToHtmlTests
    {
        private readonly TestContext ctx;
        private readonly Mock<ILocalStorageService> mockStorage;

        public MarkdownToHtmlTests()
        {
            mockStorage = new();
            ctx = new();

            ctx.Services.AddSingleton(new MarkdownPipelineBuilder().Build());
            ctx.Services.AddSingleton(mockStorage.Object);
        }

        ~MarkdownToHtmlTests()
        {
            ctx.Dispose();
        }

        [Fact(DisplayName = "Renders Correctly - Init")]
        public void RendersCorrectlyInit()
        {
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

        [Fact(DisplayName = "Restore From LocalStorage")]
        public void RestoreFromLocalStorage()
        {
            mockStorage
                .Setup(m => m.GetItemAsync<string?>(It.IsAny<string>()))
                .ReturnsAsync("test");
            var cut = ctx.RenderComponent<MarkdownToHtml>();

            var textarea = cut.Find("textarea.form-control");

            cut.WaitForAssertion(
                () => textarea.GetAttribute("value").MarkupMatches("test"),
                TimeSpan.FromMilliseconds(700)
            );

            Assert.Null(cut.Find("button.btn-danger").GetAttribute("disabled"));
            Assert.Null(cut.Find("button.btn-primary").GetAttribute("disabled"));
        }

        [Fact(DisplayName = "Text Input")]
        public void TextInput()
        {
            var cut = ctx.RenderComponent<MarkdownToHtml>();

            var textarea = cut.Find("textarea.form-control");

            textarea.Input("test");

            cut.WaitForAssertion(
                () => textarea.GetAttribute("value").MarkupMatches("test"),
                TimeSpan.FromMilliseconds(700)
            );

            Assert.Null(cut.Find("button.btn-danger").GetAttribute("disabled"));
            Assert.Null(cut.Find("button.btn-primary").GetAttribute("disabled"));
            cut.Find("div.form-control").InnerHtml.MarkupMatches("<p>test</p>");
        }

        [Fact(DisplayName = "Text Input - Null")]
        public void TextInputNull()
        {
            var cut = ctx.RenderComponent<MarkdownToHtml>();

            var textarea = cut.Find("textarea.form-control");

            textarea.Input(default(string)!);

            cut.WaitForAssertion(
                () => textarea.GetAttribute("value").MarkupMatches(""),
                TimeSpan.FromMilliseconds(700)
            );

            Assert.NotNull(cut.Find("button.btn-danger").GetAttribute("disabled"));
            Assert.NotNull(cut.Find("button.btn-primary").GetAttribute("disabled"));
            cut.Find("div.form-control").InnerHtml.MarkupMatches("");
        }

        [Fact(DisplayName = "Text Input - Blank")]
        public void TextInputBlank()
        {
            var cut = ctx.RenderComponent<MarkdownToHtml>();

            var textarea = cut.Find("textarea.form-control");

            textarea.Input(string.Empty);

            cut.WaitForAssertion(
                () => textarea.GetAttribute("value").MarkupMatches(""),
                TimeSpan.FromMilliseconds(700)
            );

            Assert.NotNull(cut.Find("button.btn-danger").GetAttribute("disabled"));
            Assert.NotNull(cut.Find("button.btn-primary").GetAttribute("disabled"));
            cut.Find("div.form-control").InnerHtml.MarkupMatches("");
        }

        [Fact(DisplayName = "Format Input")]
        public void FormatInput()
        {
            var cut = ctx.RenderComponent<MarkdownToHtml>();

            var textarea = cut.Find("textarea.form-control");
            var formatButton = cut.Find("button.btn-primary");
            const string preFormatInput =
@"# test



- a
- b";
            const string postFormatInput =
@"# test

- a
- b";

            textarea.Input(preFormatInput);

            cut.WaitForAssertion(
                () => textarea.GetAttribute("value").MarkupMatches(preFormatInput),
                TimeSpan.FromMilliseconds(700)
            );

            formatButton.Click();

            cut.WaitForAssertion(
                () => Assert.NotNull(formatButton.GetAttribute("disabled")),
                TimeSpan.FromMilliseconds(350)
            );
            cut.WaitForAssertion(
                () => textarea.GetAttribute("value").MarkupMatches(postFormatInput),
                TimeSpan.FromMilliseconds(700)
            );
            cut.WaitForAssertion(
                () => Assert.Null(formatButton.GetAttribute("disabled")),
                TimeSpan.FromMilliseconds(350)
            );
        }

        [Fact(DisplayName = "Clear Input")]
        public void ClearInput()
        {
            var cut = ctx.RenderComponent<MarkdownToHtml>();

            var textarea = cut.Find("textarea.form-control");
            const string preClearInput =
@"# test



- a
- b";

            textarea.Input(preClearInput);

            cut.WaitForAssertion(
                () => textarea.GetAttribute("value").MarkupMatches(preClearInput),
                TimeSpan.FromMilliseconds(700)
            );

            cut.Find("button.btn-danger").Click();

            cut.WaitForAssertion(
                () => textarea.GetAttribute("value").MarkupMatches(""),
                TimeSpan.FromMilliseconds(350)
            );
        }
    }
}