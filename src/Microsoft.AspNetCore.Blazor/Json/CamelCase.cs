// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Blazor.Json
{
    internal static class CamelCase
    {
        public static string ToCamelCase(this string value)
        {
            // Basic cases: if we don't need to modify the value, bail out without creating a char array
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (!char.IsUpper(value[0]))
            {
                return value;
            }

            // We have to modify at least one character
            var chars = value.ToCharArray();

            var length = chars.Length;
            if (length < 2 || !char.IsUpper(chars[1]))
            {
                // Basic cases where only the first char needs to be modified
                chars[0] = char.ToLowerInvariant(chars[0]);
            }
            else
            {
                // In the more general case we lowercase the first char plus any consecutive uppercase ones,
                // stopping if we find any char that is followed by a non-uppercase one
                var i = 0;
                while (i < length)
                {
                    chars[i] = char.ToLowerInvariant(chars[i]);

                    i++;

                    // If the next-plus-one char isn't also uppercase, then we're now on the last uppercase, so stop
                    if (i < length - 1 && !char.IsUpper(chars[i + 1]))
                    {
                        break;
                    }
                }
            }

            return new string(chars);
        }
    }
}
