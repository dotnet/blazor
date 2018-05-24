using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Blazor.Browser.Interop
{
    // Represents the information required to invoke a dotnet callback when invoking an asynchronous
    // javascript registered function.
    internal class DotNetAsync
    {
        public string CallbackId { get; set; }
        public MethodOptions FunctionName { get; set; }
    }
}
