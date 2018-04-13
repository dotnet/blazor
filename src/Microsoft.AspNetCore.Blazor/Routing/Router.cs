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
            var types = GetComponentTypesInAssembly(AppAssembly);
            Routes = RouteTable.Create(types);
            Refresh();
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

        protected virtual void Render(RenderTreeBuilder builder, Type handler, IDictionary<string, object> parameters)
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

        // Currently, the Router only looks for routable components in the application assembly,
        // not in its dependencies. That's a deliberate restriction because later we'll have a way
        // of mounting the components exposed from different assemblies at different points in the
        // URL space. It wouldn't be a good idea to mash together routable components from all
        // dependency assemblies at the root of the URL space because there'd be no sensible way
        // to manage precedence or to be sure that adding a type to an assembly doesn't break routing
        // in a different assembly. The routing config for each assembly should be encapsulated.
        private static IEnumerable<Type> GetComponentTypesInAssembly(Assembly assembly)
            => assembly.ExportedTypes.Where(t => typeof(IComponent).IsAssignableFrom(t));
    }
}
