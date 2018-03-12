using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Blazor.Server.Rendering
{
    public static class BlazorPrerenderingAppBuilderExtensions
    {
        public static void UseBlazorPrerendering<TEntryComponent>(this IApplicationBuilder app, string entryTagName, Action<IServiceCollection> configure)
        {
            app.UseBlazor<TEntryComponent>(c => c.UseBlazorPrerendering<TEntryComponent>(entryTagName, configure));
        }
    }
}