using Microsoft.AspNetCore.SpaServices;

namespace Microsoft.AspNetCore.Blazor.Server.Rendering
{
    public static class BlazorPrerenderingSpaBuilderExtensions
    {
        public static void UseBlazorPrerendering<T>(this ISpaBuilder spa)
        {
            BlazorPrerenderingMiddleware.Attach<T>(spa);
        }
    }
}