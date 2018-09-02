// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Microsoft.AspNetCore.Blazor.Razor
{
    internal class ChildContentDiagnosticPass : IntermediateNodePassBase, IRazorOptimizationPass
    {
        // Runs after components/eventhandlers/ref/bind/templates. We want to validate every component
        // and it's usage of ChildContent.
        public override int Order => 160;

        protected override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode)
        {
            var componentNodes = documentNode.FindDescendantNodes<ComponentExtensionNode>();
            for (var i = 0; i < componentNodes.Count; i++)
            {
                var componentNode = componentNodes[i];
                ValidateComponent(componentNode);
            }
        }

        private void ValidateComponent(ComponentExtensionNode node)
        {
            // Check for properties that are set by both element contents (body) and the attribute itself.
            foreach (var childContent in node.ChildContents)
            {
                foreach (var attribute in node.Attributes)
                {
                    if (attribute.AttributeName == childContent.AttributeName)
                    {
                        node.Diagnostics.Add(BlazorDiagnosticFactory.Create_ChildContentSetByAttributeAndBody(
                            attribute.Source,
                            attribute.AttributeName));
                    }
                }
            }
        }
    }
}
