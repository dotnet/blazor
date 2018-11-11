// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.Rendering;
using Microsoft.AspNetCore.Blazor.RenderTree;
using Microsoft.AspNetCore.Blazor.Routing;
using Microsoft.AspNetCore.Blazor.Services;
using Microsoft.AspNetCore.Blazor.Test.Helpers;
using Xunit;

namespace Microsoft.AspNetCore.Blazor.Test.Routing
{
    public class NavLinkTests
    {
        [Fact]
        public void DoesNotApplyActiveClassOnEmptyHref()
        {
            // Arrange
            var link = PrepareNavLinkForHref(string.Empty);

            // Act
            var isActive = (bool) typeof(NavLink).GetField("_isActive", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(link);

            // Assert
            Assert.False(isActive);
        }

        [Fact]
        public void DoesNotApplyActiveClassOnNullHref()
        {
            // Arrange
            var link = PrepareNavLinkForHref(null);

            // Act
            var isActive = (bool) typeof(NavLink).GetField("_isActive", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(link);

            // Assert
            Assert.False(isActive);
        }

        [Fact]
        public void AppliesActiveClass()
        {
            // Arrange
            var link = PrepareNavLinkForHref("/path");

            // Act
            var isActive = (bool) typeof(NavLink).GetField("_isActive", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(link);

            // Assert
            Assert.True(isActive);
        }

        private NavLink PrepareNavLinkForHref(string href)
        {
            var link = new NavLink();
            var uriHelper = new FakeUriHelper();
            var renderer = new TestRenderer();
            var handle = new RenderHandle(renderer, 0);
            var parameterCollection = new ParameterCollectionBuilder
            {
                {"href", href}
            }.Build(renderer);

            typeof(NavLink).GetProperty("UriHelper", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(link, uriHelper);
            link.Init(handle);
            link.SetParameters(parameterCollection);

            return link;
        }

        class FakeUriHelper : IUriHelper
        {
            public event EventHandler<string> OnLocationChanged;
            public string GetAbsoluteUri() => "scheme://host/path";
            public string GetBaseUri() => "scheme://host/";

            public Uri ToAbsoluteUri(string href)
            {
                return string.IsNullOrEmpty(href) ? new Uri(this.GetBaseUri()) : new Uri(new Uri(this.GetBaseUri()), href);
            }

            public void NavigateTo(string uri)
            {
                this.OnLocationChanged?.Invoke(this, uri);
            }

            public string ToBaseRelativePath(string baseUri, string locationAbsolute) => throw new NotImplementedException();
        }

        class ParameterCollectionBuilder : IEnumerable
        {
            private readonly List<(string Name, object Value)> _keyValuePairs
                = new List<(string, object)>();

            public void Add(string name, object value)
                => _keyValuePairs.Add((name, value));

            public IEnumerator GetEnumerator()
                => throw new NotImplementedException();

            public ParameterCollection Build(Renderer renderer)
            {
                var builder = new RenderTreeBuilder(renderer);
                builder.OpenComponent<NavLink>(0);
                foreach (var kvp in _keyValuePairs)
                {
                    builder.AddAttribute(1, kvp.Name, kvp.Value);
                }

                builder.CloseComponent();
                return new ParameterCollection(builder.GetFrames().Array, ownerIndex: 0);
            }
        }
    }
}
