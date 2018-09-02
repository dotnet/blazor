// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.RenderTree;
using Microsoft.AspNetCore.Blazor.Test.Helpers;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Microsoft.AspNetCore.Blazor.Build.Test
{
    public class ChildContentRazorIntegrationTest : RazorIntegrationTestBase
    {
        private readonly CSharpSyntaxTree RenderChildContentComponent = Parse(@"
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.RenderTree;
namespace Test
{
    public class RenderChildContent : BlazorComponent
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.AddContent(0, ChildContent);
        }

        [Parameter]
        RenderFragment ChildContent { get; set; }
    }
}
");

        internal override bool UseTwoPhaseCompilation => true;

        [Fact]
        public void Render_ChildContent_Body()
        {
            // Arrange
            AdditionalSyntaxTrees.Add(RenderChildContentComponent);

            var component = CompileToComponent(@"
@addTagHelper *, TestAssembly
<RenderChildContent>
  <div></div>
</RenderChildContent>");

            // Act
            var frames = GetRenderTree(component);

            // Assert
            Assert.Collection(
                frames,
                frame => AssertFrame.Component(frame, "Test.RenderChildContent", 2, 0),
                frame => AssertFrame.Attribute(frame, RenderTreeBuilder.ChildContent, 1),
                frame => AssertFrame.Markup(frame, "\n  <div></div>\n", 2));
        }

        [Fact]
        public void Render_ChildContent_Body_Recursive()
        {
            // Arrange
            AdditionalSyntaxTrees.Add(RenderChildContentComponent);

            var component = CompileToComponent(@"
@addTagHelper *, TestAssembly
<RenderChildContent>
  <RenderChildContent>
    <div></div>
  </RenderChildContent>
</RenderChildContent>");

            // Act
            var frames = GetRenderTree(component);

            // Assert
            Assert.Collection(
                frames,
                frame => AssertFrame.Component(frame, "Test.RenderChildContent", 2, 0),
                frame => AssertFrame.Attribute(frame, RenderTreeBuilder.ChildContent, 1),
                frame => AssertFrame.Whitespace(frame, 2),
                frame => AssertFrame.Component(frame, "Test.RenderChildContent", 2, 3),
                frame => AssertFrame.Attribute(frame, RenderTreeBuilder.ChildContent, 4),
                frame => AssertFrame.Whitespace(frame, 6),
                frame => AssertFrame.Markup(frame, "\n    <div></div>\n  ", 5));
        }

        [Fact]
        public void Render_ChildContent_Template()
        {
            // Arrange
            AdditionalSyntaxTrees.Add(RenderChildContentComponent);

            var component = CompileToComponent(@"
@addTagHelper *, TestAssembly
@{ RenderFragment<string> template = @<div>@context.ToLowerInvariant()</div>; }
<RenderChildContent ChildContent=""@template.WithValue(""HI"")"" />");

            // Act
            var frames = GetRenderTree(component);

            // Assert
            Assert.Collection(
                frames,
                frame => AssertFrame.Component(frame, "Test.RenderChildContent", 2, 2),
                frame => AssertFrame.Attribute(frame, RenderTreeBuilder.ChildContent, 3),
                frame => AssertFrame.Element(frame, "div", 2, 0),
                frame => AssertFrame.Text(frame, "hi", 1));
        }

        [Fact]
        public void Render_ChildContent_Template_EmptyBody()
        {
            // Arrange
            AdditionalSyntaxTrees.Add(RenderChildContentComponent);

            var component = CompileToComponent(@"
@addTagHelper *, TestAssembly
@{ RenderFragment<string> template = @<div>@context.ToLowerInvariant()</div>; }
<RenderChildContent ChildContent=""@template.WithValue(""HI"")""></RenderChildContent>");

            // Act
            var frames = GetRenderTree(component);

            // Assert
            Assert.Collection(
                frames,
                frame => AssertFrame.Component(frame, "Test.RenderChildContent", 2, 2),
                frame => AssertFrame.Attribute(frame, RenderTreeBuilder.ChildContent, 3),
                frame => AssertFrame.Element(frame, "div", 2, 0),
                frame => AssertFrame.Text(frame, "hi", 1));
        }

        [Fact]
        public void Render_ChildContent_Template_WhitespaceBody()
        {
            // Arrange
            AdditionalSyntaxTrees.Add(RenderChildContentComponent);

            var component = CompileToComponent(@"
@addTagHelper *, TestAssembly
@{ RenderFragment<string> template = @<div>@context.ToLowerInvariant()</div>; }
<RenderChildContent ChildContent=""@template.WithValue(""HI"")"">
       
</RenderChildContent>");

            // Act
            var frames = GetRenderTree(component);

            // Assert
            Assert.Collection(
                frames,
                frame => AssertFrame.Component(frame, "Test.RenderChildContent", 2, 2),
                frame => AssertFrame.Attribute(frame, RenderTreeBuilder.ChildContent, 3),
                frame => AssertFrame.Element(frame, "div", 2, 0),
                frame => AssertFrame.Text(frame, "hi", 1));
        }

        [Fact(Skip = "NYI")]
        public void Render_ChildContent_TemplateAndBody_ProducesDiagnostic()
        {
            // Arrange
            AdditionalSyntaxTrees.Add(RenderChildContentComponent);

            var component = CompileToComponent(@"
@addTagHelper *, TestAssembly
@{ RenderFragment<string> template = @<div>@context.ToLowerInvariant()</div>; }
<RenderChildContent ChildContent=""@template.WithValue(""HI"")"">
Some Content
</RenderChildContent>");

            // Act
            var frames = GetRenderTree(component);

            // Assert
            Assert.Collection(
                frames,
                frame => AssertFrame.Component(frame, "Test.RenderChildContent", 2, 2),
                frame => AssertFrame.Attribute(frame, RenderTreeBuilder.ChildContent, 3),
                frame => AssertFrame.Element(frame, "div", 2, 0),
                frame => AssertFrame.Text(frame, "hi", 1));
        }
    }
}
