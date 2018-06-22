// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.JSInterop
{
    /// <summary>
    /// Abstract base class for a JavaScript runtime.
    /// </summary>
    public abstract class JSRuntimeBase : IJSRuntime
    {
        private long _nextPendingTaskId = 1; // Start at 1 because zero signals "no response needed"
        private readonly ConcurrentDictionary<long, object> _pendingTasks
            = new ConcurrentDictionary<long, object>();

        /// <summary>
        /// Invokes the specified JavaScript function asynchronously.
        /// </summary>
        /// <typeparam name="T">The JSON-serializable return type.</typeparam>
        /// <param name="identifier">An identifier for the function to invoke. For example, the value <code>"someScope.someFunction"</code> will invoke the function <code>window.someScope.someFunction</code>.</param>
        /// <param name="args">JSON-serializable arguments.</param>
        /// <returns>An instance of <typeparamref name="T"/> obtained by JSON-deserializing the return value.</returns>
        public Task<T> InvokeAsync<T>(string identifier, params object[] args)
        {
            // TODO: Auto-timeout

            var taskId = Interlocked.Increment(ref _nextPendingTaskId);
            var tcs = new TaskCompletionSource<T>();
            _pendingTasks[taskId] = tcs;

            try
            {
                BeginInvokeJS(taskId, identifier, args?.Length > 0 ? Json.Serialize(args) : null);
                return tcs.Task;
            }
            catch
            {
                _pendingTasks.TryRemove(taskId, out _);
                throw;
            }
        }

        /// <summary>
        /// Begins an asynchronous function invocation.
        /// </summary>
        /// <param name="asyncHandle">The identifier for the function invocation, or zero if no async callback is required.</param>
        /// <param name="identifier">The identifier for the function to invoke.</param>
        /// <param name="argsJson">A JSON representation of the arguments.</param>
        protected abstract void BeginInvokeJS(long asyncHandle, string identifier, string argsJson);

        internal void EndInvokeDotNet(string callId, bool success, object resultOrExceptionMessage)
        {
            // We pass 0 as the async handle because we don't want the JS-side code to
            // send back any notification (we're just providing a result for an existing async call)
            BeginInvokeJS(0, "DotNet.jsCallDispatcher.endInvokeDotNetFromJS", Json.Serialize(new[] {
                callId,
                success,
                resultOrExceptionMessage
            }));
        }

        internal void EndInvokeJS(long asyncHandle, bool succeeded, object resultOrException)
        {
            if (!_pendingTasks.TryRemove(asyncHandle, out var tcs))
            {
                throw new ArgumentException($"There is no pending task with handle '{asyncHandle}'.");
            }

            if (succeeded)
            {
                TaskGenericsUtil.SetTaskCompletionSourceResult(tcs, resultOrException);
            }
            else
            {
                TaskGenericsUtil.SetTaskCompletionSourceException(tcs, new JSException(resultOrException.ToString()));
            }
        }
    }
}
