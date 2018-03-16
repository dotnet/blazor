// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.Layouts;
using Microsoft.AspNetCore.Blazor.RenderTree;
using Microsoft.AspNetCore.Blazor.Services;

namespace Microsoft.AspNetCore.Blazor.Routing
{
    /// <summary>
    /// A component that displays whichever other component corresponds to the
    /// current navigation location.
    /// </summary>
    public class Router : IComponent, IDisposable
    {
        static readonly char[] _queryOrHashStartChar = new[] { '?', '#' };

        RenderHandle _renderHandle;
        string _baseUriPrefix;
        string _locationAbsolute;

        [Inject] private IUriHelper UriHelper { get; set; }

        /// <summary>
        /// Gets or sets the assembly that should be searched, along with its referenced
        /// assemblies, for components matching the URI.
        /// </summary>
        public Assembly AppAssembly { get; set; }

        private RouteTable Routes { get; set; }

        /// <inheritdoc />
        public void Init(RenderHandle renderHandle)
        {
            _renderHandle = renderHandle;
            _baseUriPrefix = UriHelper.GetBaseUriPrefix();
            _locationAbsolute = UriHelper.GetAbsoluteUri();
            UriHelper.OnLocationChanged += OnLocationChanged;
        }

        /// <inheritdoc />
        public void SetParameters(ParameterCollection parameters)
        {
            parameters.AssignToProperties(this);
            var types = ResolveTypes(AppAssembly);
            Routes = RouteTable.Create(types);
            Refresh();
        }

        private Type[] ResolveTypes(Assembly appAssembly)
        {
            var comparer = new AssemblyComparer();
            var blazorAssembly = typeof(Router).Assembly;

            return EnumerateAssemblies(
                    appAssembly.GetName(),
                    new HashSet<Assembly>(comparer))
                .SelectMany(a => a.ExportedTypes).ToArray();

            IEnumerable<Assembly> EnumerateAssemblies(AssemblyName assemblyName, HashSet<Assembly> visited)
            {
                var assembly = Assembly.Load(assemblyName);
                if (visited.Contains(assembly))
                {
                    // Avoid traversing visited assemblies.
                    yield break;
                }
                visited.Add(assembly);
                var references = assembly.GetReferencedAssemblies();
                if (references.All(r => r.FullName != blazorAssembly.FullName))
                {
                    // Avoid traversing references that don't point to blazor (like netstandard2.0)
                    yield break;
                }
                else
                {
                    yield return assembly;

                    // Look at the list of transitive dependencies for more components.
                    foreach (var reference in references.SelectMany(r => EnumerateAssemblies(r, visited)))
                    {
                        yield return reference;
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

        /// <inheritdoc />
        public void Dispose()
        {
            UriHelper.OnLocationChanged -= OnLocationChanged;
        }

        private string StringUntilAny(string str, char[] chars)
        {
            var firstIndex = str.IndexOfAny(chars);
            return firstIndex < 0
                ? str
                : str.Substring(0, firstIndex);
        }

        protected virtual void Render(RenderTreeBuilder builder, Type handler, IDictionary<string, string> parameters)
        {
            builder.OpenComponent(0, typeof(LayoutDisplay));
            builder.AddAttribute(1, nameof(LayoutDisplay.Page), handler);
            builder.AddAttribute(2, nameof(LayoutDisplay.PageParameters), parameters);
            builder.CloseComponent();
        }

        private void Refresh()
        {
            var locationPath = UriHelper.ToBaseRelativePath(_baseUriPrefix, _locationAbsolute);
            locationPath = StringUntilAny(locationPath, _queryOrHashStartChar);
            var context = new RouteContext(locationPath);
            Routes.Route(context);
            if (context.Handler == null)
            {
                throw new InvalidOperationException($"'{nameof(Router)}' cannot find any component with a route for '{locationPath}'.");
            }

            if (!typeof(IComponent).IsAssignableFrom(context.Handler))
            {
                throw new InvalidOperationException($"The type {context.Handler.FullName} " +
                    $"does not implement {typeof(IComponent).FullName}.");
            }

            _renderHandle.Render(builder => Render(builder, context.Handler, context.Parameters));
        }

        private void OnLocationChanged(object sender, string newAbsoluteUri)
        {
            _locationAbsolute = newAbsoluteUri;
            if (_renderHandle.IsInitialized)
            {
                Refresh();
            }
        }
    }
}
