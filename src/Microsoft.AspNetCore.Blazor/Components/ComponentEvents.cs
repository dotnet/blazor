// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Blazor.Components
{
    /// <summary>
    /// Provides methods to invoke callbacks using component event handler semantics.
    /// That is, if the event handler target is a component, it is able to run its own
    /// logic around the callback (for example, to re-render that component afterwards).
    /// </summary>
    public static class ComponentEvents
    {
        /// <summary>
        /// Invokes the supplied callback, treating it as an event handler. This allows the target to
        /// perform event handler behaviors (e.g., for components, automatically re-rendering after the
        /// handler is invoked).
        /// </summary>
        /// <param name="handler">The event handler callback.</param>
        /// <param name="eventArgs">An argument for the event handler callback.</param>
        /// <returns>A <see cref="Task"/> representing any asynchronous work being performed as a result.</returns>
        public static Task InvokeEventHandler<TArg>(MulticastDelegate handler, TArg eventArgs)
        {
            if (handler == null)
            {
                // It's useful to allow null, because components will often take optional
                // event handler parameters. This saves the component author the trouble
                // of putting "if (SomeParam != null) { ... }" around all their calls.
                return Task.CompletedTask;
            }

            // By wrapping the handler delegate in EventHandlerInvoker<TArg>:
            // [1] We prevent further access to the underlying delegate. This is useful because it
            //     avoids possible confusion with the target calling ComponentEvents.InvokeEventHandler
            //     with it again (which would be an infinite loop).
            // [2] We capture the TArg such that when invoker.Invoke gets called later, it can check for
            //     the common case where "handler is Action<TArg>" or similar, and if so, invoke it
            //     efficiently by casting to that type and not having to using DynamicInvoke.
            var invoker = new EventHandlerInvoker<TArg>(handler);
            if (handler.Target is IHandleEvent handleEventTarget)
            {
                return handleEventTarget.HandleEvent(invoker, eventArgs);
            }
            else
            {
                // For other delegates, there's nothing to do besides invoke them directly
                return invoker.Invoke(eventArgs);
            }
        }
    }
}
