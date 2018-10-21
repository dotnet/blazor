using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Blazor.Components;

namespace Microsoft.AspNetCore.Blazor.Routing
{
    /// <summary>
    /// A collection of routes to components.
    /// </summary>
    public class RouteCollection
    {
        internal IEnumerable<RouteEntry> Routes { get; }

        internal RouteCollection(IEnumerable<RouteEntry> routes)
        {
            Routes = routes;
        }

        /// <summary>
        /// Searches a list of components to find routes.
        /// </summary>
        /// <param name="types">The components.</param>
        /// <returns></returns>
        public static RouteCollection ResolveRoutes(IEnumerable<Type> types)
        {
            var routes = new List<RouteEntry>();
            foreach (var type in types)
            {
                var routeAttributes = type.GetCustomAttributes<RouteAttribute>(inherit: true);
                foreach (var routeAttribute in routeAttributes)
                {
                    var template = TemplateParser.ParseTemplate(routeAttribute.Template);
                    var entry = new RouteEntry(template, type);
                    routes.Add(entry);
                }
            }

            return new RouteCollection(routes);
        }
    }
}
