using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Blazor.Routing
{
    internal class RouteTemplate
    {
        public static readonly char[] Separators = new[] { '/' };

        public RouteTemplate(TemplateSegment[] segments)
        {
            Segments = segments;
        }

        public TemplateSegment[] Segments { get; }

        internal TemplateMatch Match(string path)
        {
            // This is a simplification. We are assuming there are no paths like /a//b/. A proper routing
            // implementation would be more sophisticated.
            var pathSegments = path.Trim('/').Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            if (Segments.Length != pathSegments.Length)
            {
                return TemplateMatch.Fail;
            }

            var parameters = new Dictionary<string, string>();
            for (int i = 0; i < Segments.Length; i++)
            {
                var segment = Segments[i];
                var pathSegment = pathSegments[i];
                if (!segment.Match(pathSegment))
                {
                    return TemplateMatch.Fail;
                }
                else
                {
                    if (segment.IsParameter)
                    {
                        parameters[segment.Value] = pathSegment;
                    }
                }
            }

            return TemplateMatch.Succeded(parameters);
        }
    }
}
