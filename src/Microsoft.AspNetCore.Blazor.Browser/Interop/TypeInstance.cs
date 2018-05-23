using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Blazor.Browser.Interop
{
    internal class TypeInstance
    {
        public TypeInstance()
        {
        }

        public string Assembly { get; set; }
        public string TypeName { get; set; }
        public IDictionary<string, TypeInstance> TypeArguments { get; set; }

        internal Type GetTypeOrThrow()
        {
            return Type.GetType($"{TypeName}, {Assembly}", throwOnError: true);
        }
    }
}
