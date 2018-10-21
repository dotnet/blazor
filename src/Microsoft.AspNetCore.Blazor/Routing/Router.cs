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

        /// <summary>
        /// Gets or sets the render handle for this router.
        /// </summary>
        protected RenderHandle RenderHandle { get; set; }

        /// <summary>
        /// Gets or sets the base URI of this router.
        /// </summary>
        [Parameter] protected string BaseUri { get; set; }

        /// <summary>
        /// Gets or sets the current absolute location URI.
        /// </summary>
        protected string LocationAbsolute { get; set; }

        [Inject] private IUriHelper UriHelper { get; set; }

        /// <summary>
        /// Gets or sets the assembly that should be searched, along with its referenced
        /// assemblies, for components matching the URI.
        /// </summary>
        [Parameter] protected Assembly AppAssembly { get; set; }

        /// <summary>
        /// Gets or sets the route that should be displayed when another route fails to be
        /// resolved.
        /// </summary>
        [Parameter] protected string FallbackRoute { get; set; }

        /// <summary>
        /// Gets or sets the route that should be displayed when an error occuring when
        /// refreshing.
        /// </summary>
        [Parameter] protected string ErrorRoute { get; set; }

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
            _assemblyRoutes = new Dictionary<Assembly, RouteCollection>(new AssemblyComparer());
        }

        /// <inheritdoc />
        public void Init(RenderHandle renderHandle)
        {
            RenderHandle = renderHandle;
            if (BaseUri == null)
            {
                BaseUri = UriHelper.GetBaseUri();
            }
            LocationAbsolute = UriHelper.GetAbsoluteUri();
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

        /// <summary>
        /// Handles the refreshing of the router and rendering the output.
        /// </summary>
        protected virtual void Refresh()
        {
            var locationPath = UriHelper.ToBaseRelativePath(BaseUri, LocationAbsolute);
            locationPath = StringUntilAny(locationPath, _queryOrHashStartChar);

            try
            {
                ComponentResult result;
                try
                {
                    result = GetComponentForPath(locationPath);
                }
                catch (RouteNotFoundException)
                {
                    result = HandleMissingRoute();
                }

                if (result != null)
                {
                    RenderHandle.Render(builder => Render(builder, result.Handler, result.ComponentParameters));
                }
            }
            catch (Exception e)
            {
                ComponentResult result = HandleError(e);

                if (result != null)
                {
                    RenderHandle.Render(builder => Render(builder, result.Handler, result.ComponentParameters));
                }
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
        /// <exception cref="Exception">If no error route exist, the error is rethrown</exception>
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
            LocationAbsolute = newAbsoluteUri;
            if (RenderHandle.IsInitialized)
            {
                Refresh();
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
