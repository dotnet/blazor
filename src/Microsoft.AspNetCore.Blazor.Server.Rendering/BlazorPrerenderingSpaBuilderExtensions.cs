using System;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Blazor.Server.Rendering
{
    public static class BlazorPrerenderingSpaBuilderExtensions
    {
        public static void UseBlazorPrerendering<T>(this ISpaBuilder spa, string entryTagName, Action<IServiceCollection> configure)
        {
            BlazorPrerenderingMiddleware.Attach<T>(spa, entryTagName, configure);
        }
    }
}