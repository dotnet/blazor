using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.Blazor.Browser.Interop
{
    internal class MethodInstance
    {
        public MethodInstance()
        {
        }

        public string Name { get; set; }
        /// <summary>
        /// Required if the method is generic.
        /// </summary>
        public IDictionary<string, TypeInstance> TypeArguments { get; set; }

        /// <summary>
        /// Required if the method has overloads.
        /// </summary>
        public TypeInstance[] ParameterTypes { get; set; }

        internal MethodInfo GetMethodOrThrow(Type type)
        {
            var result = type.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(m => string.Equals(m.Name, Name, StringComparison.Ordinal)).ToArray();

            if (result.Length == 1)
            {
                // The method doesn't have overloads, we just return the method found by name.
                return result[0];
            }

            result = result.Where(r => r.GetParameters().Length == (ParameterTypes?.Length ?? 0)).ToArray();

            if (result.Length == 1)
            {
                // The method has only a single method with the given number of parameter types.
                return result[0];
            }

            if (result.Length == 0)
            {
                throw new InvalidOperationException($"Couldn't found a method with name '{Name}' in '{type.FullName}'.");
            }
            else
            {
                throw new InvalidOperationException($"Multiple methods with name '{Name}' and '{ParameterTypes.Length}' arguments found.");
            }
        }
    }
}
