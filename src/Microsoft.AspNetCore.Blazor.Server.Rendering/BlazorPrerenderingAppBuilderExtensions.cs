using Microsoft.AspNetCore.Builder;

namespace Microsoft.AspNetCore.Blazor.Server.Rendering
{
    public static class BlazorPrerenderingAppBuilderExtensions
    {
        public static void UseBlazorPrerendering<T>(this IApplicationBuilder app)
        {
            app.UseBlazor<T>(c => c.UseBlazorPrerendering());
        }
    }
}