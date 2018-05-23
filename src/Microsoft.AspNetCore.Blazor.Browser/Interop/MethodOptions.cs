using System.Reflection;

namespace Microsoft.AspNetCore.Blazor.Browser.Interop
{
    internal class MethodOptions
    {
        public MethodOptions()
        {
        }

        public TypeInstance Type { get; set; }
        public MethodInstance Method { get; set; }
        public Async Async { get; set; }

        internal MethodInfo GetMethodOrThrow()
        {
            var type = Type.GetTypeOrThrow();
            var method = Method.GetMethodOrThrow(type);

            return method;
        }
    }
}
