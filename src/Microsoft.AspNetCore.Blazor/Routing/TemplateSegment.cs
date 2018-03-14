using System;

namespace Microsoft.AspNetCore.Blazor.Routing
{
    internal class TemplateSegment
    {
        public TemplateSegment(string segment, bool isParameter)
        {
            Value = segment;
            IsParameter = isParameter;
        }

        // The value of the segment. The exact text to match when is a literal.
        // The parameter name when its a segment
        public string Value { get; set; }

        public bool IsParameter { get; set; }

        public bool Match(string pathSegment)
        {
            if (IsParameter)
            {
                return true;
            }
            else
            {
                return string.Equals(Value, pathSegment, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
