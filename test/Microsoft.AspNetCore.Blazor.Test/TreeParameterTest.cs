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
            var nestedComponent = FindComponent<TreeParameterConsumerComponent<string>>(batch, out var nestedComponentId);
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
            Assert.Equal(1, nestedComponent.NumRenders);
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
            var nestedComponent = FindComponent<TreeParameterConsumerComponent<string>>(firstBatch, out var nestedComponentId);
            Assert.Equal(1, nestedComponent.NumRenders);
            
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
            Assert.Equal(2, nestedComponent.NumRenders);
        }

        [Fact]
        public void NotifiesDescendantsOfUpdatedTreeParameterValuesAndPreservesDirectParameters()
        {
            // Arrange
            var providedValue = "Initial value";
            var renderer = new TestRenderer();
            var component = new TestComponent(builder =>
            {
                builder.OpenComponent<Provider<string>>(0);
                builder.AddAttribute(1, "Value", providedValue);
                builder.AddAttribute(2, RenderTreeBuilder.ChildContent, new RenderFragment(childBuilder =>
                {
                    childBuilder.OpenComponent<TreeParameterConsumerComponent<string>>(0);
                    childBuilder.AddAttribute(1, "RegularParameter", "Goodbye");
                    childBuilder.CloseComponent();
                }));
                builder.CloseComponent();
            });

            // Act 1: Initial render; capture nested component ID
            var componentId = renderer.AssignRootComponentId(component);
            component.TriggerRender();
            var firstBatch = renderer.Batches.Single();
            var nestedComponent = FindComponent<TreeParameterConsumerComponent<string>>(firstBatch, out var nestedComponentId);
            Assert.Equal(1, nestedComponent.NumRenders);

            // Act 2: Re-render provider with new value
            providedValue = "Updated value";
            component.TriggerRender();

            // Assert: We re-rendered TreeParameterConsumerComponent
            Assert.Equal(2, renderer.Batches.Count);
            var secondBatch = renderer.Batches[1];
            var nestedComponentDiff = secondBatch.DiffsByComponentId[nestedComponentId].Single();

            // The nested component was rendered with the correct parameters
            Assert.Collection(nestedComponentDiff.Edits,
                edit =>
                {
                    Assert.Equal(RenderTreeEditType.UpdateText, edit.Type);
                    Assert.Equal(0, edit.ReferenceFrameIndex); // This is the only change
                    AssertFrame.Text(secondBatch.ReferenceFrames[0], "TreeParameter=Updated value; RegularParameter=Goodbye");
                });
            Assert.Equal(2, nestedComponent.NumRenders);
        }

        [Fact]
        public void DoesNotNotifyDescendantsIfTreeParameterValuesAreImmutableAndUnchanged()
        {
            // Arrange
            var renderer = new TestRenderer();
            var component = new TestComponent(builder =>
            {
                builder.OpenComponent<Provider<string>>(0);
                builder.AddAttribute(1, "Value", "Unchanging value");
                builder.AddAttribute(2, RenderTreeBuilder.ChildContent, new RenderFragment(childBuilder =>
                {
                    childBuilder.OpenComponent<TreeParameterConsumerComponent<string>>(0);
                    childBuilder.AddAttribute(1, "RegularParameter", "Goodbye");
                    childBuilder.CloseComponent();
                }));
                builder.CloseComponent();
            });

            // Act 1: Initial render
            var componentId = renderer.AssignRootComponentId(component);
            component.TriggerRender();
            var firstBatch = renderer.Batches.Single();
            var nestedComponent = FindComponent<TreeParameterConsumerComponent<string>>(firstBatch, out _);
            Assert.Equal(3, firstBatch.DiffsByComponentId.Count); // Root + Provider + nested
            Assert.Equal(1, nestedComponent.NumRenders);

            // Act/Assert: Re-render the provider; observe nested component wasn't re-rendered
            component.TriggerRender();

            // Assert: We did not re-render TreeParameterConsumerComponent
            Assert.Equal(2, renderer.Batches.Count);
            var secondBatch = renderer.Batches[1];
            Assert.Equal(2, secondBatch.DiffsByComponentId.Count); // Root + Provider, but not nested one
            Assert.Equal(1, nestedComponent.NumRenders);
        }

        [Fact]
        public void StopsNotifyingDescendantsIfTheyAreRemoved()
        {
            // Arrange
            var providedValue = "Initial value";
            var displayNestedComponent = true;
            var renderer = new TestRenderer();
            var component = new TestComponent(builder =>
            {
                builder.OpenComponent<Provider<string>>(0);
                builder.AddAttribute(1, "Value", providedValue);
                builder.AddAttribute(2, RenderTreeBuilder.ChildContent, new RenderFragment(childBuilder =>
                {
                    if (displayNestedComponent)
                    {
                        childBuilder.OpenComponent<TreeParameterConsumerComponent<string>>(0);
                        childBuilder.AddAttribute(1, "RegularParameter", "Goodbye");
                        childBuilder.CloseComponent();
                    }
                }));
                builder.CloseComponent();
            });

            // Act 1: Initial render; capture nested component ID
            var componentId = renderer.AssignRootComponentId(component);
            component.TriggerRender();
            var firstBatch = renderer.Batches.Single();
            var nestedComponent = FindComponent<TreeParameterConsumerComponent<string>>(firstBatch, out var nestedComponentId);
            Assert.Equal(1, nestedComponent.NumSetParametersCalls);
            Assert.Equal(1, nestedComponent.NumRenders);

            // Act/Assert 2: Re-render the provider; observe nested component wasn't re-rendered
            providedValue = "Updated value";
            displayNestedComponent = false; // Remove the nested componet
            component.TriggerRender();

            // Assert: We did not render the nested component now it's been removed
            Assert.Equal(2, renderer.Batches.Count);
            var secondBatch = renderer.Batches[1];
            Assert.Equal(1, nestedComponent.NumRenders);
            Assert.Equal(2, secondBatch.DiffsByComponentId.Count); // Root + Provider, but not nested one

            // We *did* send updated params during the first render where it was removed,
            // because the params are sent before the disposal logic runs. We could avoid
            // this by moving the notifications into the OnAfterRender phase, but then we'd
            // often render descendants twice (once because they are descendants and some
            // direct parameter might have changed, then once because a cascading parameter
            // changed). We can't have it both ways, so optimize for the case when the
            // nested component *hasn't* just been removed.
            Assert.Equal(2, nestedComponent.NumSetParametersCalls);

            // Act 3: However, after disposal, the subscription is removed, so we won't send
            // updated params on subsequent provider renders.
            providedValue = "Updated value 2";
            component.TriggerRender();
            Assert.Equal(2, nestedComponent.NumSetParametersCalls);
        }

        private static T FindComponent<T>(CapturedBatch batch, out int componentId)
        {
            var componentFrame = batch.ReferenceFrames.Single(
                frame => frame.FrameType == RenderTreeFrameType.Component
                         && frame.Component is T);
            componentId = componentFrame.ComponentId;
            return (T)componentFrame.Component;
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
            public int NumSetParametersCalls { get; private set; }
            public int NumRenders { get; private set; }

            [Parameter(FromTree = true)] T TreeParameter { get; set; }
            [Parameter] string RegularParameter { get; set; }

            public override void SetParameters(ParameterCollection parameters)
            {
                NumSetParametersCalls++;
                base.SetParameters(parameters);
            }

            protected override void BuildRenderTree(RenderTreeBuilder builder)
            {
                NumRenders++;
                builder.AddContent(0, $"TreeParameter={TreeParameter}; RegularParameter={RegularParameter}");
            }
        }
    }
}
