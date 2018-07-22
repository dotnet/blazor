// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Blazor
{
    /// <summary>
    /// A string value that can be rendered as markup such as HTML.
    /// </summary>
    public struct MarkupString
    {
        private readonly string _value;

        /// <summary>
        /// Constructs an instance of <see cref="MarkupString"/>.
        /// </summary>
        /// <param name="value">The value for the new instance.</param>
        public MarkupString(string value)
        {
            _value = value ?? string.Empty;
        }

        /// <summary>
        /// Casts a <see cref="string"/> to a <see cref="MarkupString"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> value.</param>
        public static explicit operator MarkupString(string value)
            => new MarkupString(value);

        /// <inheritdoc />
        public override string ToString()
            => _value;
    }
}
