// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Blazor.Components
{
    /// <summary>
    /// Provides the ability to invoke an event handler.
    /// </summary>
    public readonly struct EventHandlerInvoker<TArg>
    {
        private readonly MulticastDelegate _delegate;

        // Internal because only the framework needs to create these. External code should
        // use ComponentEvents.InvokeEventHandler, supplying the delegate.
        internal EventHandlerInvoker(MulticastDelegate @delegate)
        {
            _delegate = @delegate;
        }

        /// <summary>
        /// Invokes the associated event handler delegate.
        /// </summary>
        /// <param name="eventArg">The argument for the event handler.</param>
        /// <returns></returns>
        public Task Invoke(TArg eventArg)
        {
            switch (_delegate)
            {
                case Action action:
                    action.Invoke();
                    return Task.CompletedTask;

                case Action<TArg> actionEventArgs:
                    actionEventArgs.Invoke(eventArg);
                    return Task.CompletedTask;

                case Func<Task> func:
                    return func.Invoke();

                case Func<TArg, Task> funcEventArgs:
                    return funcEventArgs.Invoke(eventArg);

                case MulticastDelegate @delegate:
                    return @delegate.DynamicInvoke(eventArg) as Task ?? Task.CompletedTask;

                case null:
                    return Task.CompletedTask;
            }
        }

        internal bool CheckDelegateTarget(object target)
        {
            return _delegate.Target == target;
        }
    }
}
