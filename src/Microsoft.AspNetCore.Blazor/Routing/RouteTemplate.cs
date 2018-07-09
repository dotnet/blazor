// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Blazor.Routing
{
    internal class RouteTemplate
    {
        public static readonly char[] Separators = new[] { '/' };

        public RouteTemplate(string TemplateText, TemplateSegment[] segments)
        {
            this.TemplateText = TemplateText;
            Segments = segments;
        }

        public string TemplateText { get; }

        public TemplateSegment[] Segments { get; }

        /// <summary>
        /// Returns all the parameter segment values (parameter name)       
        /// </summary>
        /// <returns></returns>

        public IEnumerable<string> GetParameterSegmentsValues()
        {
            return Segments.Where(s => s.IsParameter).Select(s => s.Value);
        }
    }
}
