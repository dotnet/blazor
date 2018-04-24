// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Blazor
{
    // TODO: Once we no longer need the Razor base class hacks, rename this from 'JsonUtil'
    // to 'Json', because it's a better name. Currently we can't call it 'Json' because the
    // fake Razor base class already has a property called 'Json'.

    /// <summary>
    /// Provides mechanisms for converting between .NET objects and JSON strings.
    /// </summary>
    public static class JsonUtil
    {
        /// <summary>
        /// Serializes the value as a JSON string using the default property naming scheme <see cref="PropertyNaming.CamelCase"/>.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <returns>The JSON string.</returns>
        public static string Serialize(object value)
            => SimpleJson.SimpleJson.SerializeObject(value);

        /// <summary>
        /// Serializes the value as a JSON string with the specified property naming scheme.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="naming">The property naming scheme for serializing.</param>
        /// <returns>The JSON string.</returns>
        public static string Serialize(object value, PropertyNaming naming)
            => SimpleJson.SimpleJson.SerializeObject(value, (int)naming);

        /// <summary>
        /// Deserializes the JSON string, creating an object of the specified generic type.
        /// </summary>
        /// <typeparam name="T">The type of object to create.</typeparam>
        /// <param name="json">The JSON string.</param>
        /// <returns>An object of the specified type.</returns>
        public static T Deserialize<T>(string json)
            => SimpleJson.SimpleJson.DeserializeObject<T>(json);

        /// <summary>
        /// Deserializes the JSON string, creating an object of the specified generic type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json">The JSON string.</param>
        /// <param name="propertyNaming">The property naming of the JSON string.</param>
        /// <returns>An object of the specified type.</returns>
        public static T Deserialize<T>(string json, PropertyNaming propertyNaming)
            => SimpleJson.SimpleJson.DeserializeObject<T>(json, (int)propertyNaming);

        /// <summary>
        /// Property Naming scheme to be used by simple json when deserializing or serializing JSON.
        /// </summary>
        public enum PropertyNaming
        {
            /// <summary>
            /// Camel case JSON property naming scheme.
            /// </summary>
            CamelCase = 1,
            /// <summary>
            /// Pascal case JSON property naming scheme.
            /// </summary>
            PascalCase = 2
        }
    }
}
