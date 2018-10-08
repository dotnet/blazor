// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.RenderTree;
using System;

namespace Microsoft.AspNetCore.Blazor.Components
{
    /// <summary>
    /// A component that provides one or more parameter values to all descendant components.
    /// </summary>
    public class Provider : IComponent
    {
        private RenderHandle _renderHandle;

        /// <summary>
        /// The content to which the value should be provided.
        /// </summary>
        [Parameter] private RenderFragment ChildContent { get; set; }

        /// <summary>
        /// The value to be provided.
        /// </summary>
        [Parameter] private object Value { get; set; }

        /// <summary>
        /// Optionally gives a name to the provided value. Descendant components
        /// will be able to receive the value by specifying this name.
        ///
        /// If no name is specified, then descendant components will receive the
        /// value based the type of value they are requesting.
        /// </summary>
        [Parameter] private string Name { get; set; }

        /// <inheritdoc />
        public void Init(RenderHandle renderHandle)
        {
            _renderHandle = renderHandle;
        }

        /// <inheritdoc />
        public void SetParameters(ParameterCollection parameters)
        {
            // Implementing the parameter binding manually, instead of just calling
            // parameters.AssignToProperties(this), is just a very slight perf optimization
            // and makes it simpler impose rules about the params being required or not.

            var hasSuppliedValue = false;
            Value = null;
            ChildContent = null;
            Name = null;

            foreach (var parameter in parameters)
            {
                if (parameter.Name.Equals(nameof(Value), StringComparison.OrdinalIgnoreCase))
                {
                    Value = parameter.Value;
                    hasSuppliedValue = true;
                }
                else if (parameter.Name.Equals(nameof(ChildContent), StringComparison.OrdinalIgnoreCase))
                {
                    ChildContent = (RenderFragment)parameter.Value;
                }
                else if (parameter.Name.Equals(nameof(Name), StringComparison.OrdinalIgnoreCase))
                {
                    Name = (string)parameter.Value;
                    if (string.IsNullOrEmpty(Name))
                    {
                        throw new ArgumentException($"The parameter '{nameof(Name)}' for component '{nameof(Provider)}' does not allow null or empty values.");
                    }
                }
                else
                {
                    throw new ArgumentException($"The component '{nameof(Provider)}' does not accept a parameter with the name '{parameter.Name}'.");
                }
            }

            // It's OK for the value to be null, but some "Value" param must be suppled
            // because it serves no useful purpose to have a <Provider> otherwise.
            if (!hasSuppliedValue)
            {
                throw new ArgumentException($"Missing required parameter '{nameof(Value)}' for component '{nameof(Parameter)}'.");
            }

            _renderHandle.Render(Render);
        }

        private void Render(RenderTreeBuilder builder)
        {
            builder.AddContent(0, ChildContent);
        }
    }
}
