// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Blazor.Routing
{
    /// <summary>
    /// Stores a list of enabled routes.
    /// </summary>
    public class RouteTable
    {
        private readonly HashSet<RouteCollection> _collections;
        internal RouteEntry[] Routes { get; private set; }

        /// <summary>
        /// Creates a new empty table.
        /// </summary>
        public RouteTable()
        {
            _collections = new HashSet<RouteCollection>();
        }

        /// <summary>
        /// Regenerates the internal ordered route list.
        /// </summary>
        public void RegenerateRouteCache()
        {
            Routes = _collections.SelectMany(x => x.Routes).Distinct().OrderBy(id => id, RoutePrecedence).ToArray();
        }

        /// <summary>
        /// Adds a route collection to the table.
        /// </summary>
        /// <param name="collection">The routes.</param>
        /// <param name="regenerateRouteCache">Whether to regenerate the ordered route list.</param>
        public void AddRoutes(RouteCollection collection, bool regenerateRouteCache = true)
        {
            _collections.Add(collection);
            if (regenerateRouteCache)
            {
                RegenerateRouteCache();
            }
        }

        /// <summary>
        /// Removes a route collection to the table.
        /// </summary>
        /// <param name="collection">The routes.</param>
        /// <param name="regenerateRouteCache">Whether to regenerate the ordered route list.</param>
        public void RemoveRoutes(RouteCollection collection, bool regenerateRouteCache = true)
        {
            _collections.Remove(collection);
            if (regenerateRouteCache)
            {
                RegenerateRouteCache();
            }
        }

        private static IComparer<RouteEntry> RoutePrecedence { get; } = Comparer<RouteEntry>.Create(RouteComparison);

        /// <summary>
        /// Route precedence algorithm.
        /// We collect all the routes and sort them from most specific to
        /// less specific. The specificity of a route is given by the specificity
        /// of its segments and the position of those segments in the route.
        /// * A literal segment is more specific than a parameter segment.
        /// * A parameter segment with more constraints is more specific than one with fewer constraints
        /// * Segment earlier in the route are evaluated before segments later in the route.
        /// For example:
        /// /Literal is more specific than /Parameter
        /// /Route/With/{parameter} is more specific than /{multiple}/With/{parameters}
        /// /Product/{id:int} is more specific than /Product/{id}
        ///
        /// Routes can be ambiguous if:
        /// They are composed of literals and those literals have the same values (case insensitive)
        /// They are composed of a mix of literals and parameters, in the same relative order and the
        /// literals have the same values.
        /// For example:
        /// * /literal and /Literal
        /// /{parameter}/literal and /{something}/literal
        /// /{parameter:constraint}/literal and /{something:constraint}/literal
        ///
        /// To calculate the precedence we sort the list of routes as follows:
        /// * Shorter routes go first.
        /// * A literal wins over a parameter in precedence.
        /// * For literals with different values (case insensitive) we choose the lexical order
        /// * For parameters with different numbers of constraints, the one with more wins
        /// If we get to the end of the comparison routing we've detected an ambiguous pair of routes.
        /// </summary>
        private static int RouteComparison(RouteEntry x, RouteEntry y)
        {
            var xTemplate = x.Template;
            var yTemplate = y.Template;
            if (xTemplate.Segments.Length != y.Template.Segments.Length)
            {
                return xTemplate.Segments.Length < y.Template.Segments.Length ? -1 : 1;
            }
            else
            {
                for (int i = 0; i < xTemplate.Segments.Length; i++)
                {
                    var xSegment = xTemplate.Segments[i];
                    var ySegment = yTemplate.Segments[i];
                    if (!xSegment.IsParameter && ySegment.IsParameter)
                    {
                        return -1;
                    }
                    if (xSegment.IsParameter && !ySegment.IsParameter)
                    {
                        return 1;
                    }

                    if (xSegment.IsParameter)
                    {
                        if (xSegment.Constraints.Length > ySegment.Constraints.Length)
                        {
                            return -1;
                        }
                        else if (xSegment.Constraints.Length < ySegment.Constraints.Length)
                        {
                            return 1;
                        }
                    }
                    else
                    {
                        var comparison = string.Compare(xSegment.Value, ySegment.Value, StringComparison.OrdinalIgnoreCase);
                        if (comparison != 0)
                        {
                            return comparison;
                        }
                    }
                }

                throw new InvalidOperationException($@"The following routes are ambiguous:
'{x.Template.TemplateText}' in '{x.Handler.FullName}'
'{y.Template.TemplateText}' in '{y.Handler.FullName}'
");
            }
        }

        internal void Route(RouteContext routeContext)
        {
            foreach (var route in Routes)
            {
                route.Match(routeContext);
                if (routeContext.Handler != null)
                {
                    return;
                }
            }
        }
    }
}
