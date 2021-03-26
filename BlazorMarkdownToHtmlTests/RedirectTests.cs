using Bunit;
using Bunit.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorMarkdownToHtml.Components.Tests
{
    public class RedirectTests
    {
        public class MockNavigationManager : NavigationManager
        {
            private readonly ITestRenderer renderer;

            public MockNavigationManager(ITestRenderer renderer)
            {
                Initialize("http://test/", "http://test/init");

                this.renderer = renderer;
            }

            protected override void NavigateToCore(string uri, bool forceLoad)
            {
                Uri = ToAbsoluteUri(uri).ToString();

                renderer.Dispatcher.InvokeAsync(() => NotifyLocationChanged(isInterceptedLink: false));
            }
        }

        private readonly TestContext ctx;

        public RedirectTests()
        {
            ctx = new();

            ctx.Services.AddSingleton<NavigationManager, MockNavigationManager>();
        }

        ~RedirectTests()
        {
            ctx.Dispose();
        }

        [Fact]
        public void NavigateTo()
        {
            var cut = ctx.RenderComponent<Redirect>();

            cut.WaitForAssertion(() => Assert.Equal("http://test/", ctx.Services.GetService<NavigationManager>().Uri));
        }
    }
}