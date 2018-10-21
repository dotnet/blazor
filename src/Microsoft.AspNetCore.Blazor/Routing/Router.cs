// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
        string _baseUri;
        string _locationAbsolute;

        [Inject] private IUriHelper UriHelper { get; set; }

        /// <summary>
        /// Gets or sets the assembly that should be searched, along with its referenced
        /// assemblies, for components matching the URI.
        /// </summary>
        [Parameter] private Assembly AppAssembly { get; set; }

        [Parameter] private string FallbackRoute { get; set; }

        [Parameter] private string ErrorRoute { get; set; }

        /// <summary>
        /// Gets the route table containing all enabled routes.
        /// </summary>
        public RouteTable RouteTable { get; }

        private readonly IDictionary<Assembly, RouteCollection> _assemblyRoutes;

        /// <summary>
        /// Creates a new router instance.
        /// </summary>
        public Router()
        {
            RouteTable = new RouteTable();
            _assemblyRoutes = new Dictionary<Assembly, RouteCollection>();
        }

        /// <inheritdoc />
        public void Init(RenderHandle renderHandle)
        {
            _renderHandle = renderHandle;
            _baseUri = UriHelper.GetBaseUri();
            _locationAbsolute = UriHelper.GetAbsoluteUri();
            UriHelper.OnLocationChanged += OnLocationChanged;
        }

        /// <inheritdoc />
        public void SetParameters(ParameterCollection parameters)
        {
            parameters.AssignToProperties(this);

            if (AppAssembly != null)
            {
                AddAssembly(AppAssembly);
            }

            Refresh();
        }

        /// <summary>
        /// Adds all routes contained within the assembly to the route table.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public void AddAssembly(Assembly assembly)
        {
            if (_assemblyRoutes.ContainsKey(assembly))
            {
                // Assembly already loaded
                return;
            }

            var components = ComponentResolver.ResolveComponents(AppAssembly);

            var routes = RouteCollection.ResolveRoutes(components);
            _assemblyRoutes.Add(assembly, routes);

            RouteTable.AddRoutes(routes);
        }

        /// <summary>
        /// Removes all routes contained within the assembly from the route table.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public void RemoveAssembly(Assembly assembly)
        {
            if (_assemblyRoutes.TryGetValue(assembly, out RouteCollection routes))
            {
                RouteTable.RemoveRoutes(routes);
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

        /// <inheritdoc />
        protected virtual void Render(RenderTreeBuilder builder, Type handler, IDictionary<string, object> parameters)
        {
            builder.OpenComponent(0, typeof(LayoutDisplay));
            builder.AddAttribute(1, LayoutDisplay.NameOfPage, handler);
            builder.AddAttribute(2, LayoutDisplay.NameOfPageParameters, parameters);
            builder.CloseComponent();
        }

        private void Refresh()
        {
            var locationPath = UriHelper.ToBaseRelativePath(_baseUri, _locationAbsolute);
            locationPath = StringUntilAny(locationPath, _queryOrHashStartChar);

            ComponentResult result;
            try
            {
                try
                {
                    result = GetComponentForPath(locationPath);
                }
                catch (RouteNotFoundException)
                {
                    result = HandleMissingRoute();
                }
            }
            catch (Exception e)
            {
                result = HandleError(e);
            }

            if (result != null)
            {
                _renderHandle.Render(builder => Render(builder, result.Handler, result.ComponentParameters));
            }
        }

        /// <summary>
        /// Resolves a path to the component to be rendered.
        /// </summary>
        /// <param name="path">The relative path.</param>
        /// <returns>The component to be rendered, or null to cancel render.</returns>
        /// <exception cref="RouteNotFoundException">Thrown when the path can not be resolved to a route.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the path is resolved to an invalid route.</exception>
        protected virtual ComponentResult GetComponentForPath(string path)
        {
            var context = new RouteContext(path);

            RouteTable.Route(context);

            if (context.Handler == null)
            {
                throw new RouteNotFoundException($"'{nameof(Router)}' cannot find any component with a route for '{path}'.");
            }

            if (!typeof(IComponent).IsAssignableFrom(context.Handler))
            {
                throw new InvalidOperationException($"The type {context.Handler.FullName} " +
                                                    $"does not implement {typeof(IComponent).FullName}.");
            }

            return new ComponentResult {Handler = context.Handler, ComponentParameters = context.Parameters};
        }

        /// <summary>
        /// Handles when a path fails to be resolved to a route.
        /// </summary>
        /// <returns>The component to be rendered, or null to cancel render.</returns>
        /// <exception cref="RouteNotFoundException">Thrown when the fallback route fails to be resolved.</exception>
        protected virtual ComponentResult HandleMissingRoute()
        {
            if (FallbackRoute == null)
                throw new RouteNotFoundException("No fallback route specified");
            return GetComponentForPath(FallbackRoute);
        }

        /// <summary>
        /// Handles when an error occurs during routing.
        /// </summary>
        /// <param name="e">The error that occured.</param>
        /// <returns>The component to be rendered, or null to cancel render.</returns>
        /// <exception cref="Exception"></exception>
        protected virtual ComponentResult HandleError(Exception e)
        {
            if (ErrorRoute == null)
                throw e;
            return GetComponentForPath(ErrorRoute);
        }

        /// <summary>
        /// Represents the result of resolving a component for a given path.
        /// </summary>
        public class ComponentResult
        {
            /// <summary>
            /// Gets or sets the handler.
            /// </summary>
            public Type Handler { get; set; }

            /// <summary>
            /// Gets or sets parameters for the selected handler.
            /// </summary>
            public IDictionary<string, object> ComponentParameters { get; set; }
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
