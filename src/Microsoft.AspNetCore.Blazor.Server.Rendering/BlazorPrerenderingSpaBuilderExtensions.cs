using System;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Blazor.Server.Rendering
{
    /// <summary>
    /// 
    /// </summary>
    public static class BlazorPrerenderingSpaBuilderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="spa"></param>
        /// <param name="entryTagName"></param>
        /// <param name="configure"></param>
        public static void UseBlazorPrerendering<T>(this ISpaBuilder spa, string entryTagName, Action<IServiceCollection> configure)
        {
            BlazorPrerenderingMiddleware.Attach<T>(spa, entryTagName, configure);
        }
    }
}