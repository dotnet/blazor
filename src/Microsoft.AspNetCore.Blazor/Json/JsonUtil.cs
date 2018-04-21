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
        /// Sets the parsing strategy for JSON, the default is <see cref="ParsingStrategy.CamelCase"/>
        /// </summary>
        public static void SetDefaultParsingStrategy(ParsingStrategy strategy)
        {
            SimpleJson.SimpleJson.SetParsingStrategy((int)strategy);
        }

        /// <summary>
        /// Serializes the value as a JSON string.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <returns>The JSON string.</returns>
        public static string Serialize(object value)
            => SimpleJson.SimpleJson.SerializeObject(value);

        /// <summary>
        /// Deserializes the JSON string, creating an object of the specified generic type.
        /// </summary>
        /// <typeparam name="T">The type of object to create.</typeparam>
        /// <param name="json">The JSON string.</param>
        /// <returns>An object of the specified type.</returns>
        public static T Deserialize<T>(string json)
            => SimpleJson.SimpleJson.DeserializeObject<T>(json);

        /// <summary>
        /// Parsing Strategy to be used by simple json when deserializing or serializing JSON.
        /// </summary>
        public enum ParsingStrategy
        {
            /// <summary>
            /// Camel case JSON parsing strategy.
            /// </summary>
            CamelCase = 1,
            /// <summary>
            /// No JSON parsing strategy.
            /// </summary>
            None = 2
        }
    }
}
