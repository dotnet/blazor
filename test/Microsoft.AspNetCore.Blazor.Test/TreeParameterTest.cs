// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.RenderTree;
using Microsoft.AspNetCore.Blazor.Test.Helpers;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.Blazor.Test
{
    public class TreeParameterTest
    {
        [Fact]
        public void PassesTreeParametersToNestedComponents()
        {
            // Arrange
            var renderer = new TestRenderer();
            var component = new TestComponent(builder =>
            {
                builder.OpenComponent<Provider<string>>(0);
                builder.AddAttribute(1, "Value", "Hello");
                builder.AddAttribute(2, RenderTreeBuilder.ChildContent, new RenderFragment(childBuilder =>
                {
                    childBuilder.OpenComponent<TreeParameterConsumerComponent<string>>(0);
                    childBuilder.AddAttribute(1, "RegularParameter", "Goodbye");
                    childBuilder.CloseComponent();
                }));
                builder.CloseComponent();
            });

            // Act/Assert
            var componentId = renderer.AssignRootComponentId(component);
            component.TriggerRender();
            var batch = renderer.Batches.Single();
            var componentFrame = batch.ReferenceFrames.Single(
                frame => frame.FrameType == RenderTreeFrameType.Component
                         && frame.Component is TreeParameterConsumerComponent<string>);
            var nestedComponentId = componentFrame.ComponentId;
            var nestedComponentDiff = batch.DiffsByComponentId[nestedComponentId].Single();

            // The nested component was rendered with the correct parameters
            Assert.Collection(nestedComponentDiff.Edits,
                edit =>
                {
                    Assert.Equal(RenderTreeEditType.PrependFrame, edit.Type);
                    AssertFrame.Text(
                        batch.ReferenceFrames[edit.ReferenceFrameIndex],
                        "TreeParameter=Hello; RegularParameter=Goodbye");
                });
        }

        [Fact]
        public void RetainsTreeParametersWhenUpdatingDirectParameters()
        {
            // Arrange
            var renderer = new TestRenderer();
            var regularParameterValue = "Initial value";
            var component = new TestComponent(builder =>
            {
                builder.OpenComponent<Provider<string>>(0);
                builder.AddAttribute(1, "Value", "Hello");
                builder.AddAttribute(2, RenderTreeBuilder.ChildContent, new RenderFragment(childBuilder =>
                {
                    childBuilder.OpenComponent<TreeParameterConsumerComponent<string>>(0);
                    childBuilder.AddAttribute(1, "RegularParameter", regularParameterValue);
                    childBuilder.CloseComponent();
                }));
                builder.CloseComponent();
            });

            // Act 1: Render in initial state
            var componentId = renderer.AssignRootComponentId(component);
            component.TriggerRender();

            // Capture the nested component so we can verify the update later
            var firstBatch = renderer.Batches.Single();
            var componentFrame = firstBatch.ReferenceFrames.Single(
                frame => frame.FrameType == RenderTreeFrameType.Component
                         && frame.Component is TreeParameterConsumerComponent<string>);
            var nestedComponentId = componentFrame.ComponentId;

            // Act 2: Render again with updated regular parameter
            regularParameterValue = "Changed value";
            component.TriggerRender();

            // Assert
            Assert.Equal(2, renderer.Batches.Count);
            var secondBatch = renderer.Batches[1];
            var nestedComponentDiff = secondBatch.DiffsByComponentId[nestedComponentId].Single();

            // The nested component was rendered with the correct parameters
            Assert.Collection(nestedComponentDiff.Edits,
                edit =>
                {
                    Assert.Equal(RenderTreeEditType.UpdateText, edit.Type);
                    Assert.Equal(0, edit.ReferenceFrameIndex); // This is the only change
                    AssertFrame.Text(secondBatch.ReferenceFrames[0], "TreeParameter=Hello; RegularParameter=Changed value");
                });
        }

        class TestComponent : AutoRenderComponent
        {
            private readonly RenderFragment _renderFragment;

            public TestComponent(RenderFragment renderFragment)
            {
                _renderFragment = renderFragment;
            }

            protected override void BuildRenderTree(RenderTreeBuilder builder)
                => _renderFragment(builder);
        }

        class TreeParameterConsumerComponent<T> : AutoRenderComponent
        {
            [Parameter(FromTree = true)] T TreeParameter { get; set; }
            [Parameter] string RegularParameter { get; set; }

            protected override void BuildRenderTree(RenderTreeBuilder builder)
            {
                builder.AddContent(0, $"TreeParameter={TreeParameter}; RegularParameter={RegularParameter}");
            }
        }
    }
}
