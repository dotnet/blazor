using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Blazor.Server.Rendering
{
    /// <summary>
    /// 
    /// </summary>
    public static class BlazorPrerenderingAppBuilderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntryComponent"></typeparam>
        /// <param name="app"></param>
        /// <param name="entryTagName"></param>
        /// <param name="configure"></param>
        public static void UseBlazorPrerendering<TEntryComponent>(this IApplicationBuilder app, string entryTagName, Action<IServiceCollection> configure)
        {
            app.UseBlazor<TEntryComponent>(c => c.UseBlazorPrerendering<TEntryComponent>(entryTagName, configure));
        }
    }
}