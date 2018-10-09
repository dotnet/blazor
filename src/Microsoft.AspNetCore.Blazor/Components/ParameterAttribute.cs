// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Blazor.Components
{
    /// <summary>
    /// Denotes the target member as a component parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class ParameterAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value to indicate whether the parameter's value should
        /// be read from the closest ancestor Provider in the tree.
        ///
        /// If false (the default), the parameter's value can be supplied only by
        /// the immediate parent component.
        /// </summary>
        public bool FromTree { get; set; }

        /// <summary>
        /// When <see cref="FromTree"/> is true, gets or sets the name of the
        /// provided value. When <see cref="FromTree"/> is false, this value is
        /// not used.
        ///
        /// If a value is specified, the parameter value will be supplied by the
        /// closest ancestor Provider that supplies a value with this name.
        ///
        /// If no value is supplied, the parameter value will be supplied by the
        /// closest ancestor Provider that supplies a value with a compatible type.
        /// </summary>
        public string ProviderName { get; set; }
    }
}
