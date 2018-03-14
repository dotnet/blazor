using System.Collections.Generic;

namespace Microsoft.AspNetCore.Blazor.Routing
{
    internal class TemplateMatch
    {
        public static TemplateMatch Fail { get; } = new TemplateMatch
        {
            Success = false,
            Parameters = null
        };

        public static TemplateMatch Succeded(IDictionary<string, string> parameters)
        {
            return new TemplateMatch
            {
                Success = true,
                Parameters = parameters
            };
        }

        public bool Success { get; set; }

        public IDictionary<string, string> Parameters { get; set; }
    }
}
