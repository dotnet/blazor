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
    public static class ComponentResolver
    {
        private static readonly IDictionary<Assembly, IList<Type>> _componentCache =
            new Dictionary<Assembly, IList<Type>>(new AssemblyComparer());

        /// <summary>
        /// Lists all the types 
        /// </summary>
        /// <param name="appAssembly"></param>
        /// <returns></returns>
        public static IEnumerable<Type> ResolveComponents(Assembly appAssembly)
        {
            var blazorAssembly = typeof(IComponent).Assembly;

            var assemblies = new HashSet<Assembly>(new AssemblyComparer());
            SearchAssemblies(appAssembly.GetName(), blazorAssembly, new HashSet<Assembly>(new AssemblyComparer()),
                assemblies);

            var types = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                if (!_componentCache.TryGetValue(assembly, out IList<Type> components))
                {
                    components = assembly.ExportedTypes.Where(t => typeof(IComponent).IsAssignableFrom(t)).ToList();
                    _componentCache.Add(assembly, components);
                }

                types.AddRange(components);
            }

            return types;
        }

        private static bool SearchAssemblies(
            AssemblyName assemblyName,
            Assembly blazorAssembly,
            HashSet<Assembly> visited,
            HashSet<Assembly> toScan)
        {
            var assembly = Assembly.Load(assemblyName);

            if (visited.Contains(assembly))
            {
                // Avoid traversing visited assemblies.
                return toScan.Contains(assembly);
            }
            visited.Add(assembly);

            var references = assembly.GetReferencedAssemblies();
            var needsScanning = false;
            foreach (var reference in references)
            {
                // Check if assembly references blazor either directly or indirectly
                if (string.Equals(reference.FullName, blazorAssembly.FullName, StringComparison.Ordinal) ||
                    SearchAssemblies(reference, blazorAssembly, visited, toScan))
                {
                    needsScanning = true;
                }
            }

            // Marks assembly as needing to be scanned
            if (needsScanning)
            {
                toScan.Add(assembly);
            }

            return needsScanning;
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
