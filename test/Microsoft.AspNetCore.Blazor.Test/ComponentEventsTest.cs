// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.Components;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Blazor.Test
{
    public class ComponentEventsTest
    {
        [Fact]
        public void IfDelegateIsNull_ReturnsCompletedTask()
        {
            var result = ComponentEvents.InvokeEventHandler(null, new object());
            Assert.Same(Task.CompletedTask, result);
        }

        [Fact]
        public void IfDelegateIsOnIHandleEvent_CallsHandleEvent()
        {
            // Arrange
            var target = new TestHandleEvent();
            var expectedArg = new TestArg();

            // Act
            var result = ComponentEvents.InvokeEventHandler((Action<TestArg>)target.MyMethod, expectedArg);

            // Assert
            Assert.True(target.DidCallHandleEvent);
            Assert.Same(expectedArg, target.ReceivedArg);
            Assert.Same(Task.CompletedTask, result);
        }

        [Fact]
        public void IfDelegateIsOnIHandleEvent_CallsHandleEventAsync()
        {
            // Arrange
            var target = new TestHandleEvent();
            var expectedArg = new TestArg();

            // Act
            var result = ComponentEvents.InvokeEventHandler((Func<TestArg, Task>)target.MyMethodAsync, expectedArg);

            // Assert
            Assert.True(target.DidCallHandleEvent);
            Assert.Same(expectedArg, target.ReceivedArg);
            Assert.Same(target.ExpectedTask, result);
        }

        [Fact]
        public void IfDelegateIsNotOnIHandleEvent_InvokesWithoutArg()
        {
            // Arrange
            var didInvoke = false;
            Action handler = () => { didInvoke = true; };

            // Act
            var result = ComponentEvents.InvokeEventHandler(handler, "ignored arg");

            // Assert
            Assert.True(didInvoke);
            Assert.Same(Task.CompletedTask, result);
        }

        [Fact]
        public void IfDelegateIsNotOnIHandleEvent_InvokesWithArg()
        {
            // Arrange
            TestArg receivedArg = null;
            var expectedArg = new TestArg();
            Action<TestArg> handler = arg => { receivedArg = arg; };

            // Act
            var result = ComponentEvents.InvokeEventHandler(handler, expectedArg);

            // Assert
            Assert.Same(expectedArg, receivedArg);
            Assert.Same(Task.CompletedTask, result);
        }

        [Fact]
        public void IfDelegateIsNotOnIHandleEvent_InvokesWithArgLateBound()
        {
            // In this case where the delegate type isn't statically known to accept
            // the arg type but does so at runtime, it has to go through the slower
            // DynamicInvoke path but it still works.

            // Arrange
            object receivedArg = null;
            var expectedArg = new TestArgSubtype();
            Action<TestArgSubtype> handler = arg => { receivedArg = arg; };

            // Act
            var result = ComponentEvents.InvokeEventHandler(handler, (TestArg)expectedArg);

            // Assert
            Assert.Same(expectedArg, receivedArg);
            Assert.Same(Task.CompletedTask, result);
        }

        [Fact]
        public void IfDelegateIsNotOnIHandleEvent_InvokesAsyncWithoutArg()
        {
            // Arrange
            var didInvoke = false;
            var expectedTask = new Task(() => { });
            Func<Task> handler = () => { didInvoke = true; return expectedTask; };

            // Act
            var result = ComponentEvents.InvokeEventHandler(handler, "ignored arg");

            // Assert
            Assert.True(didInvoke);
            Assert.Same(expectedTask, result);
        }

        [Fact]
        public void IfDelegateIsNotOnIHandleEvent_InvokesAsyncWithArg()
        {
            // Arrange
            TestArg receivedArg = null;
            var expectedArg = new TestArg();
            var expectedTask = new Task(() => { });
            Func<TestArg, Task> handler = arg => { receivedArg = arg; return expectedTask; };

            // Act
            var result = ComponentEvents.InvokeEventHandler(handler, expectedArg);

            // Assert
            Assert.Same(expectedArg, receivedArg);
            Assert.Same(expectedTask, result);
        }

        [Fact]
        public void IfDelegateIsNotOnIHandleEvent_InvokesAsyncWithArgLateBound()
        {
            // In this case where the delegate type isn't statically known to accept
            // the arg type but does so at runtime, it has to go through the slower
            // DynamicInvoke path but it still works.

            // Arrange
            TestArgSubtype receivedArg = null;
            var expectedArg = new TestArgSubtype();
            var expectedTask = new Task(() => { });
            Func<TestArgSubtype, Task> handler = arg => { receivedArg = arg; return expectedTask; };

            // Act
            var result = ComponentEvents.InvokeEventHandler(handler, (TestArg)expectedArg);

            // Assert
            Assert.Same(expectedArg, receivedArg);
            Assert.Same(expectedTask, result);
        }

        class TestArg { }
        class TestArgSubtype : TestArg { }

        class TestHandleEvent : IHandleEvent
        {
            public bool DidCallHandleEvent { get; private set; }
            public TestArg ReceivedArg { get; private set; }
            public Task ExpectedTask { get; } = new Task(() => { });

            public void MyMethod(TestArg arg)
            {
                ReceivedArg = arg;
            }

            public Task MyMethodAsync(TestArg arg)
            {
                ReceivedArg = arg;
                return ExpectedTask;
            }

            Task IHandleEvent.HandleEvent<TArg>(EventHandlerInvoker<TArg> binding, TArg eventArgs)
            {
                DidCallHandleEvent = true;
                return binding.Invoke(eventArgs);
            }
        }
    }
}
