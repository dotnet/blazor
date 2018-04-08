// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace Microsoft.AspNetCore.Blazor
{
    public static class UIEventHandlerRenderTreeBuilderExtensions
    {
        /// <summary>
        /// <para>
        /// Appends a frame representing an <see cref="UIChangeEventArgs"/>-valued attribute.
        /// </para>
        /// <para>
        /// The attribute is associated with the most recently added element. If the value is <c>null</c> and the
        /// current element is not a component, the frame will be omitted.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="RenderTreeBuilder"/>.</param>
        /// <param name="sequence">An integer that represents the position of the instruction in the source code.</param>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        public static void AddAttribute(this RenderTreeBuilder builder, int sequence, string name, UIChangeEventHandler value)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddAttribute(sequence, name, (MulticastDelegate)value);
        }

        /// <summary>
        /// <para>
        /// Appends a frame representing an <see cref="UIKeyboardEventHandler"/>-valued attribute.
        /// </para>
        /// <para>
        /// The attribute is associated with the most recently added element. If the value is <c>null</c> and the
        /// current element is not a component, the frame will be omitted.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="RenderTreeBuilder"/>.</param>
        /// <param name="sequence">An integer that represents the position of the instruction in the source code.</param>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        public static void AddAttribute(this RenderTreeBuilder builder, int sequence, string name, UIKeyboardEventHandler value)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddAttribute(sequence, name, (MulticastDelegate)value);
        }

        /// <summary>
        /// <para>
        /// Appends a frame representing an <see cref="UIMouseEventHandler"/>-valued attribute.
        /// </para>
        /// <para>
        /// The attribute is associated with the most recently added element. If the value is <c>null</c> and the
        /// current element is not a component, the frame will be omitted.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="RenderTreeBuilder"/>.</param>
        /// <param name="sequence">An integer that represents the position of the instruction in the source code.</param>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        public static void AddAttribute(this RenderTreeBuilder builder, int sequence, string name, UIMouseEventHandler value)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddAttribute(sequence, name, (MulticastDelegate)value);
        }
    }
}
