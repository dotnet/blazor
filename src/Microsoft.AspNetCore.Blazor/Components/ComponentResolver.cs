// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.Blazor.Components
{
    /// <summary>
    /// Resolves components for an application.
    /// </summary>
    internal class ComponentResolver
    {
        /// <summary>
        /// Lists all the types 
        /// </summary>
        /// <param name="appAssembly"></param>
        /// <returns></returns>
        public static IEnumerable<Type> ResolveComponents(Assembly appAssembly)
        {
            var blazorAssembly = typeof(IComponent).Assembly;

            return EnumerateAssemblies(appAssembly.GetName(), blazorAssembly)
                .SelectMany(a => a.ExportedTypes)
                .Where(t => typeof(IComponent).IsAssignableFrom(t));
        }

        private static IEnumerable<Assembly> EnumerateAssemblies(
            AssemblyName mainAssemblyName,
            Assembly blazorAssembly)
        {
            var visited = new HashSet<Assembly>(new AssemblyComparer());
            var candidates = new HashSet<Assembly>(new AssemblyComparer());

            EnumerateAssembliesCore(mainAssemblyName);

            return candidates;

            void EnumerateAssembliesCore(AssemblyName assemblyName)
            {
                var assembly = Assembly.Load(assemblyName);
                if (visited.Contains(assembly))
                {
                    return;
                }

                visited.Add(assembly);
                var references = assembly.GetReferencedAssemblies();
                if (references.Any(r => string.Equals(r.FullName, blazorAssembly.FullName, StringComparison.Ordinal)))
                {
                    candidates.Add(assembly);
                }

                foreach (var reference in references)
                {
                    var countBeforeScanningDependencies = candidates.Count;
                    EnumerateAssembliesCore(reference);
                    // If the number of candidates has increased after this call, it means that a transitive dependency
                    // of this assembly references the Blazor dll and by that token we should consider the current
                    // assembly a candidate for components even though it might not directly reference the blazor assembly.
                    if (countBeforeScanningDependencies < candidates.Count)
                    {
                        // This is ok as it will only be added once.
                        candidates.Add(assembly);
                    }
                }
            }
        }

        private class AssemblyComparer : IEqualityComparer<Assembly>
        {
            public bool Equals(Assembly x, Assembly y)
            {
                return string.Equals(x?.FullName, y?.FullName, StringComparison.Ordinal);
            }

            public int GetHashCode(Assembly obj)
            {
                return obj.FullName.GetHashCode();
            }
        }
    }
}
