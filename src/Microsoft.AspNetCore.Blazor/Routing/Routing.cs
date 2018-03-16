using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Blazor.Routing
{
    // Helpers for doing routing for pages.
    // This implementation is temporary, in the future we'll want to have
    // a more performant/properly design routing set of abstractions.
    // To be more precise these are some things we are scoping out:
    // * We are not building a route table/tree.
    //   * We are just parsing the route on every request, matching it and
    //     extracting the parameters.
    // * We are not resolving conflicts.
    // * We are not doing link generation.
    // * We are not supporting route constraints.
    // The class in here just takes care of parsing a route and extracting
    // simple parameters from it.
    // Some differences with ASP.NET Core routes are:
    // * We don't support catch all parameter segments.
    // * We don't support optional parameter segments.
    // * We don't support complex segments.
    // The things that we support are:
    // * Literal path segments. (Like /Path/To/Some/Page)
    // * Parameter path segments (Like /Customer/{Id}/Orders/{OrderId})
    internal class Routing
    {
        public static readonly char[] InvalidParameterNameCharacters =
            new char[] { '*', '?', '{', '}', '=', '.', ':' };

        public static IDictionary<string, string> MatchRoute(
            string routeTemplate,
            string path)
        {
            var cleanTemplate = CleanTemplate(routeTemplate);
            var template = ParseTemplate(cleanTemplate);

            var match = template.Match(path);
            if (match.Success)
            {
                return match.Parameters;
            }
            else
            {
                return null;
            }
        }

        internal static RouteTemplate ParseTemplate(string template)
        {
            if (template == "")
            {
                // Special case "/";
                return new RouteTemplate(Array.Empty<TemplateSegment>());
            }

            var segments = template.Split('/');
            var templateSegments = new TemplateSegment[segments.Length];
            for (int i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                if (string.IsNullOrEmpty(segment))
                {
                    throw new InvalidOperationException(
                        $"Invalid template '{template}'. Empty segments are not allowed.");
                }

                if (segment[0] != '{')
                {
                    if (segment[segment.Length - 1] == '}')
                    {
                        throw new InvalidOperationException(
                            $"Invalid template '{template}'. Missing '{{' in parameter segment '{segment}'.");
                    }
                    templateSegments[i] = new TemplateSegment(segment, isParameter: false);
                }
                else
                {
                    if (segment[segment.Length - 1] != '}')
                    {
                        throw new InvalidOperationException(
                            $"Invalid template '{template}'. Missing '}}' in parameter segment '{segment}'.");
                    }

                    if (segment.Length < 3)
                    {
                        throw new InvalidOperationException(
                            $"Invalid template '{template}'. Empty parameter name in segment '{segment}' is not allowed.");
                    }

                    var invalidCharacter = segment.IndexOfAny(InvalidParameterNameCharacters, 1, segment.Length - 2);
                    if (invalidCharacter != -1)
                    {
                        throw new InvalidOperationException(
                            $"Invalid template '{template}'. The character '{segment[invalidCharacter]}' in parameter segment '{segment}' is not allowed.");
                    }

                    templateSegments[i] = new TemplateSegment(segment.Substring(1, segment.Length - 2), isParameter: true);
                }
            }

            for (int i = 0; i < templateSegments.Length; i++)
            {
                var currentSegment = templateSegments[i];
                if (!currentSegment.IsParameter)
                {
                    continue;
                }

                for (int j = i + 1; j < templateSegments.Length; j++)
                {
                    var nextSegment = templateSegments[j];
                    if (!nextSegment.IsParameter)
                    {
                        continue;
                    }

                    if (string.Equals(currentSegment.Value, nextSegment.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            $"Invalid template '{template}'. The parameter '{currentSegment}' appears multiple times.");
                    }
                }
            }

            return new RouteTemplate(templateSegments);
        }

        private static string CleanTemplate(string template)
        {
            return template.Trim('/');
        }
    }
}
