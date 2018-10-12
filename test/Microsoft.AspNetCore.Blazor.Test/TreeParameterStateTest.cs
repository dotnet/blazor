// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.Rendering;
using Microsoft.AspNetCore.Blazor.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.Blazor.Test
{
    public class TreeParameterStateTest
    {
        [Fact]
        public void FindTreeParameters_IfHasNoParameters_ReturnsNull()
        {
            // Arrange
            var componentState = CreateComponentState(new ComponentWithNoParams());

            // Act
            var result = TreeParameterState.FindTreeParameters(componentState);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindTreeParameters_IfHasNoTreeParameters_ReturnsNull()
        {
            // Arrange
            var componentState = CreateComponentState(new ComponentWithNoTreeParams());

            // Act
            var result = TreeParameterState.FindTreeParameters(componentState);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindTreeParameters_IfHasNoAncestors_ReturnsNull()
        {
            // Arrange
            var componentState = CreateComponentState(new ComponentWithTreeParams());

            // Act
            var result = TreeParameterState.FindTreeParameters(componentState);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindTreeParameters_IfHasNoMatchesInAncestors_ReturnsNull()
        {
            // Arrange: Build the ancestry list
            var states = CreateAncestry(
                new ComponentWithNoParams(),
                CreateProvider("Hello"),
                new ComponentWithNoParams(),
                new ComponentWithTreeParams());

            // Act
            var result = TreeParameterState.FindTreeParameters(states.Last());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindTreeParameters_IfHasPartialMatchesInAncestors_ReturnsMatches()
        {
            // Arrange
            var states = CreateAncestry(
                new ComponentWithNoParams(),
                CreateProvider(new TreeValue2()),
                new ComponentWithNoParams(),
                new ComponentWithTreeParams());

            // Act
            var result = TreeParameterState.FindTreeParameters(states.Last());

            // Assert
            Assert.Collection(result, match =>
            {
                Assert.Equal(nameof(ComponentWithTreeParams.TreeParam2), match.LocalName);
                Assert.Same(states[1].Component, match.FromProvider);
            });
        }

        [Fact]
        public void FindTreeParameters_IfHasMultipleMatchesInAncestors_ReturnsMatches()
        {
            // Arrange
            var states = CreateAncestry(
                new ComponentWithNoParams(),
                CreateProvider(new TreeValue2()),
                new ComponentWithNoParams(),
                CreateProvider(new TreeValue1()),
                new ComponentWithTreeParams());

            // Act
            var result = TreeParameterState.FindTreeParameters(states.Last());

            // Assert
            Assert.Collection(result.OrderBy(x => x.LocalName),
                match => {
                    Assert.Equal(nameof(ComponentWithTreeParams.TreeParam1), match.LocalName);
                    Assert.Same(states[3].Component, match.FromProvider);
                },
                match => {
                    Assert.Equal(nameof(ComponentWithTreeParams.TreeParam2), match.LocalName);
                    Assert.Same(states[1].Component, match.FromProvider);
                });
        }

        [Fact]
        public void FindTreeParameters_InheritedParameters_ReturnsMatches()
        {
            // Arrange
            var states = CreateAncestry(
                CreateProvider(new TreeValue1()),
                CreateProvider(new TreeValue3()),
                new ComponentWithInheritedTreeParams());

            // Act
            var result = TreeParameterState.FindTreeParameters(states.Last());

            // Assert
            Assert.Collection(result.OrderBy(x => x.LocalName),
                match => {
                    Assert.Equal(nameof(ComponentWithTreeParams.TreeParam1), match.LocalName);
                    Assert.Same(states[0].Component, match.FromProvider);
                },
                match => {
                    Assert.Equal(nameof(ComponentWithInheritedTreeParams.TreeParam3), match.LocalName);
                    Assert.Same(states[1].Component, match.FromProvider);
                });
        }

        [Fact]
        public void FindTreeParameters_ComponentRequestsBaseType_ReturnsMatches()
        {
            // Arrange
            var states = CreateAncestry(
                CreateProvider(new TreeValueDerivedClass()),
                new ComponentWithGenericTreeParam<TreeValueBaseClass>());

            // Act
            var result = TreeParameterState.FindTreeParameters(states.Last());

            // Assert
            Assert.Collection(result, match => {
                Assert.Equal(nameof(ComponentWithGenericTreeParam<object>.LocalName), match.LocalName);
                Assert.Same(states[0].Component, match.FromProvider);
            });
        }

        [Fact]
        public void FindTreeParameters_ComponentRequestsImplementedInterface_ReturnsMatches()
        {
            // Arrange
            var states = CreateAncestry(
                CreateProvider(new TreeValueDerivedClass()),
                new ComponentWithGenericTreeParam<ITreeValueDerivedClassInterface>());

            // Act
            var result = TreeParameterState.FindTreeParameters(states.Last());

            // Assert
            Assert.Collection(result, match => {
                Assert.Equal(nameof(ComponentWithGenericTreeParam<object>.LocalName), match.LocalName);
                Assert.Same(states[0].Component, match.FromProvider);
            });
        }

        [Fact]
        public void FindTreeParameters_ComponentRequestsDerivedType_ReturnsNull()
        {
            // Arrange
            var states = CreateAncestry(
                CreateProvider(new TreeValueBaseClass()),
                new ComponentWithGenericTreeParam<TreeValueDerivedClass>());

            // Act
            var result = TreeParameterState.FindTreeParameters(states.Last());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindTreeParameters_TypeAssignmentIsValidForNullValue_ReturnsMatches()
        {
            // Arrange
            var states = CreateAncestry(
                CreateProvider((TreeValueDerivedClass)null),
                new ComponentWithGenericTreeParam<TreeValueBaseClass>());

            // Act
            var result = TreeParameterState.FindTreeParameters(states.Last());

            // Assert
            Assert.Collection(result, match => {
                Assert.Equal(nameof(ComponentWithGenericTreeParam<object>.LocalName), match.LocalName);
                Assert.Same(states[0].Component, match.FromProvider);
            });
        }

        [Fact]
        public void FindTreeParameters_TypeAssignmentIsInvalidForNullValue_ReturnsNull()
        {
            // Arrange
            var states = CreateAncestry(
                CreateProvider((object)null),
                new ComponentWithGenericTreeParam<TreeValue1>());

            // Act
            var result = TreeParameterState.FindTreeParameters(states.Last());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindTreeParameters_ProviderSpecifiesNameButConsumerDoesNot_ReturnsNull()
        {
            // Arrange
            var states = CreateAncestry(
                CreateProvider(new TreeValue1(), "MatchOnName"),
                new ComponentWithTreeParams());

            // Act
            var result = TreeParameterState.FindTreeParameters(states.Last());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindTreeParameters_ConsumerSpecifiesNameButProviderDoesNot_ReturnsNull()
        {
            // Arrange
            var states = CreateAncestry(
                CreateProvider(new TreeValue1()),
                new ComponentWithNamedTreeParam());

            // Act
            var result = TreeParameterState.FindTreeParameters(states.Last());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindTreeParameters_MismatchingNameButMatchingType_ReturnsNull()
        {
            // Arrange
            var states = CreateAncestry(
                CreateProvider(new TreeValue1(), "MismatchName"),
                new ComponentWithNamedTreeParam());

            // Act
            var result = TreeParameterState.FindTreeParameters(states.Last());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindTreeParameters_MatchingNameButMismatchingType_ReturnsNull()
        {
            // Arrange
            var states = CreateAncestry(
                CreateProvider(new TreeValue2(), "MatchOnName"),
                new ComponentWithNamedTreeParam());

            // Act
            var result = TreeParameterState.FindTreeParameters(states.Last());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindTreeParameters_MatchingNameAndType_ReturnsMatches()
        {
            // Arrange
            var states = CreateAncestry(
                CreateProvider(new TreeValue1(), "matchonNAME"), // To show it's case-insensitive
                new ComponentWithNamedTreeParam());

            // Act
            var result = TreeParameterState.FindTreeParameters(states.Last());

            // Assert
            Assert.Collection(result, match => {
                Assert.Equal(nameof(ComponentWithNamedTreeParam.SomeLocalName), match.LocalName);
                Assert.Same(states[0].Component, match.FromProvider);
            });
        }

        [Fact]
        public void FindTreeParameters_MultipleMatchingAncestors_ReturnsClosestMatches()
        {
            // Arrange
            var states = CreateAncestry(
                CreateProvider(new TreeValue1()),
                CreateProvider(new TreeValue2()),
                CreateProvider(new TreeValue1()),
                CreateProvider(new TreeValue2()),
                new ComponentWithTreeParams());

            // Act
            var result = TreeParameterState.FindTreeParameters(states.Last());

            // Assert
            Assert.Collection(result.OrderBy(x => x.LocalName),
                match => {
                    Assert.Equal(nameof(ComponentWithTreeParams.TreeParam1), match.LocalName);
                    Assert.Same(states[2].Component, match.FromProvider);
                },
                match => {
                    Assert.Equal(nameof(ComponentWithTreeParams.TreeParam2), match.LocalName);
                    Assert.Same(states[3].Component, match.FromProvider);
                });
        }

        static ComponentState[] CreateAncestry(params IComponent[] components)
        {
            var result = new ComponentState[components.Length];

            for (var i = 0; i < components.Length; i++)
            {
                result[i] = CreateComponentState(
                    components[i],
                    i == 0 ? null : result[i - 1]);
            }

            return result;
        }

        static ComponentState CreateComponentState(
            IComponent component, ComponentState parentComponentState = null)
        {
            return new ComponentState(new TestRenderer(), 0, component, parentComponentState);
        }

        static Provider<T> CreateProvider<T>(T value, string name = null)
        {
            var provider = new Provider<T>();
            provider.Init(new RenderHandle(new TestRenderer(), 0));

            var providerParams = new Dictionary<string, object>
            {
                { "Value", value }
            };

            if (name != null)
            {
                providerParams.Add("Name", name);
            }

            provider.SetParameters(providerParams);
            return provider;
        }
       
        class ComponentWithNoParams : TestComponentBase
        {
        }

        class ComponentWithNoTreeParams : TestComponentBase
        {
            [Parameter] bool SomeRegularParameter { get; set; }
        }

        class ComponentWithTreeParams : TestComponentBase
        {
            [Parameter] bool RegularParam { get; set; }
            [Parameter(FromTree = true)] internal TreeValue1 TreeParam1 { get; set; }
            [Parameter(FromTree = true)] internal TreeValue2 TreeParam2 { get; set; }
        }

        class ComponentWithInheritedTreeParams : ComponentWithTreeParams
        {
            [Parameter(FromTree = true)] internal TreeValue3 TreeParam3 { get; set; }
        }

        class ComponentWithGenericTreeParam<T> : TestComponentBase
        {
            [Parameter(FromTree = true)] internal T LocalName { get; set; }
        }

        class ComponentWithNamedTreeParam : TestComponentBase
        {
            [Parameter(FromTree = true, ProviderName = "MatchOnName")]
            internal TreeValue1 SomeLocalName { get; set; }
        }

        class TestComponentBase : IComponent
        {
            public void Init(RenderHandle renderHandle)
                => throw new NotImplementedException();

            public void SetParameters(ParameterCollection parameters)
                => throw new NotImplementedException();
        }

        class TreeValue1 { }
        class TreeValue2 { }
        class TreeValue3 { }

        class TreeValueBaseClass { }
        class TreeValueDerivedClass : TreeValueBaseClass, ITreeValueDerivedClassInterface { }
        interface ITreeValueDerivedClassInterface { }
    }
}
