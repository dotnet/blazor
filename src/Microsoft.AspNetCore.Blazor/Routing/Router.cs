// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.Layouts;
using Microsoft.AspNetCore.Blazor.RenderTree;
using Microsoft.AspNetCore.Blazor.Services;
using System;
using System.Collections.Generic;
using System.Reflection;

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

        /// <summary>
        /// Gets or sets the route that should be used when no components with are found with the requested route.
        /// </summary>
        [Parameter] private string FallbackRoute { get; set; }

        private RouteTable Routes { get; set; }

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
            IEnumerable<Type> types = ComponentResolver.ResolveComponents(AppAssembly);
            Routes = RouteTable.Create(types);
            Refresh();
        }

        /// <inheritdoc />
        public void Dispose() => UriHelper.OnLocationChanged -= OnLocationChanged;

        private string StringUntilAny(string str, char[] chars)
        {
            int firstIndex = str.IndexOfAny(chars);
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

        private void Refresh(bool useFallback = false)
        {
            try
            {
                string locationPath = useFallback ? FallbackRoute : UriHelper.ToBaseRelativePath(_baseUri, _locationAbsolute);
                locationPath = StringUntilAny(locationPath, _queryOrHashStartChar);
                RouteContext context = new RouteContext(locationPath);
                Routes.Route(context);
                if (context.Handler == null)
                {
                    if (!useFallback && FallbackRoute != null)
                    {
                        Refresh(useFallback: true);
                        return;
                    }
                    else
                        throw new InvalidOperationException($"'{nameof(Router)}' cannot find any component with {(useFallback ? "the fallback route" : "a route for")} '/{locationPath}'.");
                }

                if (!typeof(IComponent).IsAssignableFrom(context.Handler))
                {
                    throw new InvalidOperationException($"The type {context.Handler.FullName} " +
                        $"does not implement {typeof(IComponent).FullName}.");
                }

                _renderHandle.Render(builder => Render(builder, context.Handler, context.Parameters));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"There was an exception in this for some reason: {ex.ToString()}");
            }
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
