using Microsoft.AspNetCore.Http;
using Xunit;

namespace BlazorMarkdownToHtml.Pages.Tests
{
    public class ErrorModelTests
    {
        private readonly ErrorModel model;

        public ErrorModelTests()
        {
            model = new();
        }

        [Fact(DisplayName = "ShowRequestId - Empty")]
        public void ShowRequestIdEmpty() => Assert.False(model.ShowRequestId);

        [Fact(DisplayName = "ShowRequestId - Filled")]
        public void ShowRequestIdFilled()
        {
            model.RequestId = "id";
            Assert.True(model.ShowRequestId);
        }

        [Fact(DisplayName = "OnGet - RequestId from HttpContext")]
        public void OnGetRequestIdFromHttpContext()
        {
            model.PageContext = new(
                new(
                    new DefaultHttpContext(),
                    new(),
                    new()
                )
            );
            model.HttpContext.TraceIdentifier = "id";

            model.OnGet();

            Assert.Equal("id", model.RequestId);
        }
    }
}