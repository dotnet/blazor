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

        /// <summary>
        /// Gets or sets the component name that will be used if the URI ends with
        /// a slash.
        /// </summary>
        public string DefaultComponentName { get; set; } = "Index";

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
            Refresh();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            UriHelper.OnLocationChanged -= OnLocationChanged;
        }

        protected virtual ComponentMatch MatchComponent(string locationPath)
        {
            if (AppAssembly == null)
            {
                throw new InvalidOperationException($"No value was specified for {nameof(AppAssembly)}.");
            }

            locationPath = StringUntilAny(locationPath, _queryOrHashStartChar);

            return MatchComponentOnAssemblyOrReferences(AppAssembly, locationPath)
                ?? throw new InvalidOperationException($"'{nameof(Router)}' cannot find any component with a route for '{locationPath}'.");
        }

        private string StringUntilAny(string str, char[] chars)
        {
            var firstIndex = str.IndexOfAny(chars);
            return firstIndex < 0
                ? str
                : str.Substring(0, firstIndex);
        }

        private ComponentMatch MatchComponentOnAssemblyOrReferences(Assembly assembly, string route)
            => MatchComponentRoute(assembly, route)
            ?? assembly.GetReferencedAssemblies()
                .Select(Assembly.Load)
                .Select(referencedAssembly => MatchComponentOnAssemblyOrReferences(referencedAssembly, route))
                .FirstOrDefault();

        private ComponentMatch MatchComponentRoute(Assembly assembly, string route)
        {
            foreach (var type in assembly.ExportedTypes.Where(t => typeof(IComponent).IsAssignableFrom(t)))
            {
                var routes = type.GetCustomAttributes<RouteAttribute>();
                foreach (var candidate in routes)
                {
                    var parameters = Routing.MatchRoute(candidate.Template, route);
                    if (parameters != null)
                    {
                        return new ComponentMatch(type, parameters);
                    }
                }
            }

            return null;
        }

        protected virtual void Render(RenderTreeBuilder builder, ComponentMatch match)
        {
            builder.OpenComponent(0, typeof(LayoutDisplay));
            builder.AddAttribute(1, nameof(LayoutDisplay.Page), match.Handler);
            builder.AddAttribute(2, nameof(LayoutDisplay.PageParameters), match.Parameters);
            builder.CloseComponent();
        }

        private void Refresh()
        {
            var locationPath = UriHelper.ToBaseRelativePath(_baseUriPrefix, _locationAbsolute);
            var matchedComponentType = MatchComponent(locationPath);
            if (!typeof(IComponent).IsAssignableFrom(matchedComponentType.Handler))
            {
                throw new InvalidOperationException($"The type {matchedComponentType.Handler.FullName} " +
                    $"does not implement {typeof(IComponent).FullName}.");
            }

            _renderHandle.Render(builder => Render(builder, matchedComponentType));
        }

        public class ComponentMatch
        {
            public ComponentMatch(Type component, IDictionary<string, string> parameters)
            {
                Handler = component;
                Parameters = parameters;
            }

            public Type Handler { get; set; }

            public IDictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
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
