// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.RenderTree;
using Microsoft.AspNetCore.Blazor.Test.Helpers;
using Xunit;

namespace Microsoft.AspNetCore.Blazor.Test
{
    public class ParameterCollectionAssignmentExtensionsTest
    {
        [Fact]
        public void IncomingParameterMatchesDeclaredParameter_SetsValue()
        {
            // Arrange
            var someObject = new object();
            var parameterCollection = new ParameterCollectionBuilder
            {
                { nameof(HasPublicInstanceProperties.IntProp), 123 },
                { nameof(HasPublicInstanceProperties.StringProp), "Hello" },
                { nameof(HasPublicInstanceProperties.ObjectProp), someObject },
            }.Build();
            var target = new HasPublicInstanceProperties();

            // Act
            parameterCollection.AssignToProperties(target);

            // Assert
            Assert.Equal(123, target.IntProp);
            Assert.Equal("Hello", target.StringProp);
            Assert.Same(someObject, target.ObjectProp);
        }

        [Fact]
        public void NoIncomingParameterMatchesDeclaredParameter_LeavesValueUnchanged()
        {
            // Arrange
            var existingObjectValue = new object();
            var target = new HasPublicInstanceProperties
            {
                IntProp = 456,
                StringProp = "Existing value",
                ObjectProp = existingObjectValue
            };

            var parameterCollection = new ParameterCollectionBuilder().Build();

            // Act
            parameterCollection.AssignToProperties(target);

            // Assert
            Assert.Equal(456, target.IntProp);
            Assert.Equal("Existing value", target.StringProp);
            Assert.Same(existingObjectValue, target.ObjectProp);
        }

        [Fact]
        public void IncomingParameterMatchesNoDeclaredParameter_Throws()
        {
            // Arrange
            var target = new HasPropertyWithoutParameterAttribute();
            var parameterCollection = new ParameterCollectionBuilder
            {
                { "AnyOtherKey", 123 },
            }.Build();

            // Act
            var ex = Assert.Throws<InvalidOperationException>(
                () => parameterCollection.AssignToProperties(target));

            // Assert
            Assert.Equal(
                $"Object of type '{typeof(HasPropertyWithoutParameterAttribute).FullName}' does not have a property " +
                $"matching the name 'AnyOtherKey'.",
                ex.Message);
        }

        [Fact]
        public void IncomingParameterMatchesPropertyNotDeclaredAsParameter_Throws()
        {
            // Arrange
            var target = new HasPropertyWithoutParameterAttribute();
            var parameterCollection = new ParameterCollectionBuilder
            {
                { nameof(HasPropertyWithoutParameterAttribute.IntProp), 123 },
            }.Build();

            // Act
            var ex = Assert.Throws<InvalidOperationException>(
                () => parameterCollection.AssignToProperties(target));

            // Assert
            Assert.Equal(default, target.IntProp);
            Assert.Equal(
                $"Object of type '{typeof(HasPropertyWithoutParameterAttribute).FullName}' has a property matching the name '{nameof(HasPropertyWithoutParameterAttribute.IntProp)}', " +
                $"but it does not have [{nameof(ParameterAttribute)}] applied.",
                ex.Message);
        }

        [Fact]
        public void IncomingParameterValueMismatchesDeclaredParameterType_Throws()
        {
            // Arrange
            var someObject = new object();
            var parameterCollection = new ParameterCollectionBuilder
            {
                { nameof(HasPublicInstanceProperties.IntProp), "string value" },
            }.Build();
            var target = new HasPublicInstanceProperties();

            // Act
            var ex = Assert.Throws<InvalidOperationException>(
                () => parameterCollection.AssignToProperties(target));

            // Assert
            Assert.Equal(
                $"Unable to set property '{nameof(HasPublicInstanceProperties.IntProp)}' on object of " +
                $"type '{typeof(HasPublicInstanceProperties).FullName}'. The error was: {ex.InnerException.Message}",
                ex.Message);
        }

        [Fact]
        public void PropertyExplicitSetterException_Throws()
        {
            // Arrange
            var target = new HasPropertyWhoseSetterThrows();
            var parameterCollection = new ParameterCollectionBuilder
            {
                { nameof(HasPropertyWhoseSetterThrows.StringProp), "anything" },
            }.Build();

            // Act
            var ex = Assert.Throws<InvalidOperationException>(
                () => parameterCollection.AssignToProperties(target));

            // Assert
            Assert.Equal(
                $"Unable to set property '{nameof(HasPropertyWhoseSetterThrows.StringProp)}' on object of " +
                $"type '{typeof(HasPropertyWhoseSetterThrows).FullName}'. The error was: {ex.InnerException.Message}",
                ex.Message);
        }

        class HasPublicInstanceProperties
        {
            [Parameter] public int IntProp { get; set; }
            [Parameter] public string StringProp { get; set; }
            [Parameter] public object ObjectProp { get; set; }
        }

        class HasPropertyWithoutParameterAttribute
        {
            public int IntProp { get; set; }
        }

        class HasPropertyWhoseSetterThrows
        {
            [Parameter]
            public string StringProp
            {
                get => string.Empty;
                set => throw new InvalidOperationException("This setter throws");
            }
        }

        class ParameterCollectionBuilder : IEnumerable
        {
            private readonly List<(string Name, object Value)> _keyValuePairs
                = new List<(string, object)>();

            public void Add(string name, object value)
                => _keyValuePairs.Add((name, value));

            public IEnumerator GetEnumerator()
                => throw new NotImplementedException();

            public ParameterCollection Build()
            {
                var builder = new RenderTreeBuilder(new TestRenderer());
                builder.OpenComponent<FakeComponent>(0);
                foreach (var kvp in _keyValuePairs)
                {
                    builder.AddAttribute(1, kvp.Name, kvp.Value);
                }
                builder.CloseComponent();
                return new ParameterCollection(builder.GetFrames().Array, ownerIndex: 0);
            }
        }

        class FakeComponent : IComponent
        {
            public void Init(RenderHandle renderHandle)
                => throw new NotImplementedException();

            public void SetParameters(ParameterCollection parameters)
                => throw new NotImplementedException();
        }
    }
}
