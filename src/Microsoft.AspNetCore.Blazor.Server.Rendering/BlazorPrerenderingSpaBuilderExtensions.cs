using Microsoft.AspNetCore.SpaServices;

namespace Microsoft.AspNetCore.Blazor.Server.Rendering
{
    public static class BlazorPrerenderingSpaBuilderExtensions
    {
        public static void UseBlazorPrerendering(this ISpaBuilder spa)
        {
            BlazorPrerenderingMiddleware.Attach(spa);
        }
    }
}