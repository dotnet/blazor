// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Blazor
{
    /// <summary>
    /// fetch() request credentials mapping
    /// </summary>
    public enum RequestCredentials
    {
        /// <summary>
        /// same-origin
        /// </summary>
        [Description("same-origin")]
        SameOrigin,

        /// <summary>
        /// omit
        /// </summary>
        [Description("omit")]
        Omit,

        /// <summary>
        /// include
        /// </summary>
        [Description("include")]
        Include
    }

    /// <summary>
    /// Extension methods for working with JSON APIs.
    /// </summary>
    public static class HttpClientJsonExtensions
    {
        /// <summary>
        /// Sends a GET request to the specified URI, and parses the JSON response body
        /// to create an object of the generic type.
        /// </summary>
        /// <typeparam name="T">A type into which the response body can be JSON-deserialized.</typeparam>
        /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
        /// <param name="requestUri">The URI that the request will be sent to.</param>
        /// <param name="requestCredentials">If specified, override the default behavior.</param>
        /// <returns>The response parsed as an object of the generic type.</returns>
        public static Task<T> GetJsonAsync<T>(this HttpClient httpClient, string requestUri, RequestCredentials? requestCredentials = null)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
            return httpClient.SendAsync<T>(message, requestCredentials);
        }

        /// <summary>
        /// Sends a POST request to the specified URI, including the specified <paramref name="content"/>
        /// in JSON-encoded format, and parses the JSON response body to create an object of the generic type.
        /// </summary>
        /// <typeparam name="T">A type into which the response body can be JSON-deserialized.</typeparam>
        /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
        /// <param name="requestUri">The URI that the request will be sent to.</param>
        /// <param name="content">Content for the request body. This will be JSON-encoded and sent as a string.</param>
        /// <param name="requestCredentials">If specified, override the default behavior.</param>
        /// <returns>The response parsed as an object of the generic type.</returns>
        public static Task PostJsonAsync(this HttpClient httpClient, string requestUri, object content, RequestCredentials? requestCredentials = null)
            => httpClient.SendJsonAsync(HttpMethod.Post, requestUri, content, requestCredentials);

        /// <summary>
        /// Sends a POST request to the specified URI, including the specified <paramref name="content"/>
        /// in JSON-encoded format, and parses the JSON response body to create an object of the generic type.
        /// </summary>
        /// <typeparam name="T">A type into which the response body can be JSON-deserialized.</typeparam>
        /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
        /// <param name="requestUri">The URI that the request will be sent to.</param>
        /// <param name="content">Content for the request body. This will be JSON-encoded and sent as a string.</param>
        /// <param name="requestCredentials">If specified, override the default behavior.</param>
        /// <returns>The response parsed as an object of the generic type.</returns>
        public static Task<T> PostJsonAsync<T>(this HttpClient httpClient, string requestUri, object content, RequestCredentials? requestCredentials = null)
            => httpClient.SendJsonAsync<T>(HttpMethod.Post, requestUri, content, requestCredentials);

        /// <summary>
        /// Sends a PUT request to the specified URI, including the specified <paramref name="content"/>
        /// in JSON-encoded format.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
        /// <param name="requestUri">The URI that the request will be sent to.</param>
        /// <param name="requestCredentials">If specified, override the default behavior.</param>
        /// <param name="content">Content for the request body. This will be JSON-encoded and sent as a string.</param>
        public static Task PutJsonAsync(this HttpClient httpClient, string requestUri, object content, RequestCredentials? requestCredentials = null)
            => httpClient.SendJsonAsync(HttpMethod.Put, requestUri, content, requestCredentials);

        /// <summary>
        /// Sends a PUT request to the specified URI, including the specified <paramref name="content"/>
        /// in JSON-encoded format, and parses the JSON response body to create an object of the generic type.
        /// </summary>
        /// <typeparam name="T">A type into which the response body can be JSON-deserialized.</typeparam>
        /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
        /// <param name="requestUri">The URI that the request will be sent to.</param>
        /// <param name="content">Content for the request body. This will be JSON-encoded and sent as a string.</param>
        /// <param name="requestCredentials">If specified, override the default behavior.</param>
        /// <returns>The response parsed as an object of the generic type.</returns>
        public static Task<T> PutJsonAsync<T>(this HttpClient httpClient, string requestUri, object content, RequestCredentials? requestCredentials = null)
            => httpClient.SendJsonAsync<T>(HttpMethod.Put, requestUri, content, requestCredentials);

        /// <summary>
        /// Sends an HTTP request to the specified URI, including the specified <paramref name="content"/>
        /// in JSON-encoded format.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">The URI that the request will be sent to.</param>
        /// <param name="content">Content for the request body. This will be JSON-encoded and sent as a string.</param>
        /// <param name="requestCredentials">If specified, override the default behavior.</param>
        public static Task SendJsonAsync(this HttpClient httpClient, HttpMethod method, string requestUri, object content, RequestCredentials? requestCredentials = null)
            => httpClient.SendJsonAsync<IgnoreResponse>(method, requestUri, content, requestCredentials);

        /// <summary>
        /// Sends an HTTP request to the specified URI, including the specified <paramref name="content"/>
        /// in JSON-encoded format, and parses the JSON response body to create an object of the generic type.
        /// </summary>
        /// <typeparam name="T">A type into which the response body can be JSON-deserialized.</typeparam>
        /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">The URI that the request will be sent to.</param>
        /// <param name="content">Content for the request body. This will be JSON-encoded and sent as a string.</param>
        /// <param name="requestCredentials">If specified, override the default behavior.</param>
        /// <returns>The response parsed as an object of the generic type.</returns>
        public static Task<T> SendJsonAsync<T>(this HttpClient httpClient, HttpMethod method, string requestUri, object content, RequestCredentials? requestCredentials = null)
        {
            var requestJson = JsonUtil.Serialize(content);

            var message = new HttpRequestMessage(method, requestUri)
            {
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            };

            return httpClient.SendAsync<T>(message, requestCredentials);
        }

        /// <summary>
        /// Sends an HTTP request to the specified URI, including the specified <paramref name="content"/>
        /// in JSON-encoded format, and parses the JSON response body to create an object of the generic type.
        /// </summary>
        /// <typeparam name="T">A type into which the response body can be JSON-deserialized.</typeparam>
        /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
        /// <param name="message"><see cref="HttpRequestMessage"/> to send.</param>
        /// <param name="requestCredentials">If specified, override the default behavior.</param>
        /// <returns>The response parsed as an object of the generic type.</returns>
        public static async Task<T> SendAsync<T>(this HttpClient httpClient, HttpRequestMessage message, RequestCredentials? requestCredentials = null)
        {
            HttpResponseMessage response = await httpClient.SendAsync(message, requestCredentials);

            if (typeof(T) == typeof(IgnoreResponse))
            {
                return default;
            }
            else
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonUtil.Deserialize<T>(responseJson);
            }
        }

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
        /// <param name="message"><see cref="HttpRequestMessage"/> to send.</param>
        /// <param name="requestCredentials">If specified, override the default behavior.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public static async Task<HttpResponseMessage> SendAsync(this HttpClient httpClient, HttpRequestMessage message, RequestCredentials? requestCredentials)
        {
            if (requestCredentials.HasValue)
            {
                message.Properties.Add("RequestCredentials", requestCredentials);
            }

            var response = await httpClient.SendAsync(message);
            return response;
        }

        class IgnoreResponse { }
    }
}
