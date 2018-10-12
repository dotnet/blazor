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
    public class CascadingParameterStateTest
    {
        [Fact]
        public void FindCascadingParameters_IfHasNoParameters_ReturnsNull()
        {
            // Arrange
            var componentState = CreateComponentState(new ComponentWithNoParams());

            // Act
            var result = CascadingParameterState.FindCascadingParameters(componentState);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindCascadingParameters_IfHasNoCascadingParameters_ReturnsNull()
        {
            // Arrange
            var componentState = CreateComponentState(new ComponentWithNoTreeParams());

            // Act
            var result = CascadingParameterState.FindCascadingParameters(componentState);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindCascadingParameters_IfHasNoAncestors_ReturnsNull()
        {
            // Arrange
            var componentState = CreateComponentState(new ComponentWithTreeParams());

            // Act
            var result = CascadingParameterState.FindCascadingParameters(componentState);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindCascadingParameters_IfHasNoMatchesInAncestors_ReturnsNull()
        {
            // Arrange: Build the ancestry list
            var states = CreateAncestry(
                new ComponentWithNoParams(),
                CreateCascadingValueComponent("Hello"),
                new ComponentWithNoParams(),
                new ComponentWithTreeParams());

            // Act
            var result = CascadingParameterState.FindCascadingParameters(states.Last());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindCascadingParameters_IfHasPartialMatchesInAncestors_ReturnsMatches()
        {
            // Arrange
            var states = CreateAncestry(
                new ComponentWithNoParams(),
                CreateCascadingValueComponent(new TreeValue2()),
                new ComponentWithNoParams(),
                new ComponentWithTreeParams());

            // Act
            var result = CascadingParameterState.FindCascadingParameters(states.Last());

            // Assert
            Assert.Collection(result, match =>
            {
                Assert.Equal(nameof(ComponentWithTreeParams.TreeParam2), match.LocalValueName);
                Assert.Same(states[1].Component, match.ValueSupplier);
            });
        }

        [Fact]
        public void FindCascadingParameters_IfHasMultipleMatchesInAncestors_ReturnsMatches()
        {
            // Arrange
            var states = CreateAncestry(
                new ComponentWithNoParams(),
                CreateCascadingValueComponent(new TreeValue2()),
                new ComponentWithNoParams(),
                CreateCascadingValueComponent(new TreeValue1()),
                new ComponentWithTreeParams());

            // Act
            var result = CascadingParameterState.FindCascadingParameters(states.Last());

            // Assert
            Assert.Collection(result.OrderBy(x => x.LocalValueName),
                match => {
                    Assert.Equal(nameof(ComponentWithTreeParams.TreeParam1), match.LocalValueName);
                    Assert.Same(states[3].Component, match.ValueSupplier);
                },
                match => {
                    Assert.Equal(nameof(ComponentWithTreeParams.TreeParam2), match.LocalValueName);
                    Assert.Same(states[1].Component, match.ValueSupplier);
                });
        }

        [Fact]
        public void FindCascadingParameters_InheritedParameters_ReturnsMatches()
        {
            // Arrange
            var states = CreateAncestry(
                CreateCascadingValueComponent(new TreeValue1()),
                CreateCascadingValueComponent(new TreeValue3()),
                new ComponentWithInheritedTreeParams());

            // Act
            var result = CascadingParameterState.FindCascadingParameters(states.Last());

            // Assert
            Assert.Collection(result.OrderBy(x => x.LocalValueName),
                match => {
                    Assert.Equal(nameof(ComponentWithTreeParams.TreeParam1), match.LocalValueName);
                    Assert.Same(states[0].Component, match.ValueSupplier);
                },
                match => {
                    Assert.Equal(nameof(ComponentWithInheritedTreeParams.TreeParam3), match.LocalValueName);
                    Assert.Same(states[1].Component, match.ValueSupplier);
                });
        }

        [Fact]
        public void FindCascadingParameters_ComponentRequestsBaseType_ReturnsMatches()
        {
            // Arrange
            var states = CreateAncestry(
                CreateCascadingValueComponent(new TreeValueDerivedClass()),
                new ComponentWithGenericTreeParam<TreeValueBaseClass>());

            // Act
            var result = CascadingParameterState.FindCascadingParameters(states.Last());

            // Assert
            Assert.Collection(result, match => {
                Assert.Equal(nameof(ComponentWithGenericTreeParam<object>.LocalName), match.LocalValueName);
                Assert.Same(states[0].Component, match.ValueSupplier);
            });
        }

        [Fact]
        public void FindCascadingParameters_ComponentRequestsImplementedInterface_ReturnsMatches()
        {
            // Arrange
            var states = CreateAncestry(
                CreateCascadingValueComponent(new TreeValueDerivedClass()),
                new ComponentWithGenericTreeParam<ITreeValueDerivedClassInterface>());

            // Act
            var result = CascadingParameterState.FindCascadingParameters(states.Last());

            // Assert
            Assert.Collection(result, match => {
                Assert.Equal(nameof(ComponentWithGenericTreeParam<object>.LocalName), match.LocalValueName);
                Assert.Same(states[0].Component, match.ValueSupplier);
            });
        }

        [Fact]
        public void FindCascadingParameters_ComponentRequestsDerivedType_ReturnsNull()
        {
            // Arrange
            var states = CreateAncestry(
                CreateCascadingValueComponent(new TreeValueBaseClass()),
                new ComponentWithGenericTreeParam<TreeValueDerivedClass>());

            // Act
            var result = CascadingParameterState.FindCascadingParameters(states.Last());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindCascadingParameters_TypeAssignmentIsValidForNullValue_ReturnsMatches()
        {
            // Arrange
            var states = CreateAncestry(
                CreateCascadingValueComponent((TreeValueDerivedClass)null),
                new ComponentWithGenericTreeParam<TreeValueBaseClass>());

            // Act
            var result = CascadingParameterState.FindCascadingParameters(states.Last());

            // Assert
            Assert.Collection(result, match => {
                Assert.Equal(nameof(ComponentWithGenericTreeParam<object>.LocalName), match.LocalValueName);
                Assert.Same(states[0].Component, match.ValueSupplier);
            });
        }

        [Fact]
        public void FindCascadingParameters_TypeAssignmentIsInvalidForNullValue_ReturnsNull()
        {
            // Arrange
            var states = CreateAncestry(
                CreateCascadingValueComponent((object)null),
                new ComponentWithGenericTreeParam<TreeValue1>());

            // Act
            var result = CascadingParameterState.FindCascadingParameters(states.Last());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindCascadingParameters_SupplierSpecifiesNameButConsumerDoesNot_ReturnsNull()
        {
            // Arrange
            var states = CreateAncestry(
                CreateCascadingValueComponent(new TreeValue1(), "MatchOnName"),
                new ComponentWithTreeParams());

            // Act
            var result = CascadingParameterState.FindCascadingParameters(states.Last());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindCascadingParameters_ConsumerSpecifiesNameButSupplierDoesNot_ReturnsNull()
        {
            // Arrange
            var states = CreateAncestry(
                CreateCascadingValueComponent(new TreeValue1()),
                new ComponentWithNamedTreeParam());

            // Act
            var result = CascadingParameterState.FindCascadingParameters(states.Last());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindCascadingParameters_MismatchingNameButMatchingType_ReturnsNull()
        {
            // Arrange
            var states = CreateAncestry(
                CreateCascadingValueComponent(new TreeValue1(), "MismatchName"),
                new ComponentWithNamedTreeParam());

            // Act
            var result = CascadingParameterState.FindCascadingParameters(states.Last());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindCascadingParameters_MatchingNameButMismatchingType_ReturnsNull()
        {
            // Arrange
            var states = CreateAncestry(
                CreateCascadingValueComponent(new TreeValue2(), "MatchOnName"),
                new ComponentWithNamedTreeParam());

            // Act
            var result = CascadingParameterState.FindCascadingParameters(states.Last());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindCascadingParameters_MatchingNameAndType_ReturnsMatches()
        {
            // Arrange
            var states = CreateAncestry(
                CreateCascadingValueComponent(new TreeValue1(), "matchonNAME"), // To show it's case-insensitive
                new ComponentWithNamedTreeParam());

            // Act
            var result = CascadingParameterState.FindCascadingParameters(states.Last());

            // Assert
            Assert.Collection(result, match => {
                Assert.Equal(nameof(ComponentWithNamedTreeParam.SomeLocalName), match.LocalValueName);
                Assert.Same(states[0].Component, match.ValueSupplier);
            });
        }

        [Fact]
        public void FindCascadingParameters_MultipleMatchingAncestors_ReturnsClosestMatches()
        {
            // Arrange
            var states = CreateAncestry(
                CreateCascadingValueComponent(new TreeValue1()),
                CreateCascadingValueComponent(new TreeValue2()),
                CreateCascadingValueComponent(new TreeValue1()),
                CreateCascadingValueComponent(new TreeValue2()),
                new ComponentWithTreeParams());

            // Act
            var result = CascadingParameterState.FindCascadingParameters(states.Last());

            // Assert
            Assert.Collection(result.OrderBy(x => x.LocalValueName),
                match => {
                    Assert.Equal(nameof(ComponentWithTreeParams.TreeParam1), match.LocalValueName);
                    Assert.Same(states[2].Component, match.ValueSupplier);
                },
                match => {
                    Assert.Equal(nameof(ComponentWithTreeParams.TreeParam2), match.LocalValueName);
                    Assert.Same(states[3].Component, match.ValueSupplier);
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

        static CascadingValue<T> CreateCascadingValueComponent<T>(T value, string name = null)
        {
            var supplier = new CascadingValue<T>();
            supplier.Init(new RenderHandle(new TestRenderer(), 0));

            var supplierParams = new Dictionary<string, object>
            {
                { "Value", value }
            };

            if (name != null)
            {
                supplierParams.Add("Name", name);
            }

            supplier.SetParameters(supplierParams);
            return supplier;
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
