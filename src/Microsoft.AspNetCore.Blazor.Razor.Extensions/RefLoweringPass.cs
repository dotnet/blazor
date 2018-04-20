// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using System.Linq;

namespace Microsoft.AspNetCore.Blazor.Razor
{
    internal class RefLoweringPass : IntermediateNodePassBase, IRazorOptimizationPass
    {
        protected override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode)
        {
            var @namespace = documentNode.FindPrimaryNamespace();
            var @class = documentNode.FindPrimaryClass();
            if (@namespace == null || @class == null)
            {
                // Nothing to do, bail. We can't function without the standard structure.
                return;
            }

            var nodes = documentNode.FindDescendantNodes<TagHelperIntermediateNode>();
            var isDesignTime = documentNode.Options.DesignTime;
            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];

                for (var j = node.Children.Count - 1; j >= 0; j--)
                {
                    var attributeNode = node.Children[j] as ComponentAttributeExtensionNode;
                    if (attributeNode != null &&
                        attributeNode.TagHelper != null &&
                        attributeNode.TagHelper.IsRefTagHelper())
                    {
                        RewriteUsage(@class, node, j, attributeNode, isDesignTime);
                    }
                }
            }
        }

        private void RewriteUsage(ClassDeclarationIntermediateNode classNode, TagHelperIntermediateNode node, int index, ComponentAttributeExtensionNode attributeNode, bool isDesignTime)
        {
            node.Children.Remove(attributeNode);

            // If we can't get a nonempty attribute name, do nothing because there will
            // already be a diagnostic for empty values
            var identifierToken = DetermineIdentifierToken(attributeNode);
            if (identifierToken != null)
            {
                // Determine whether this is an element capture or a component capture, and
                // if applicable the type name that will appear in the resulting capture code
                var componentTagHelper = node.TagHelpers.FirstOrDefault(x => x.IsComponentTagHelper());
                var refExtensionNode = componentTagHelper == null
                    ? new RefExtensionNode(identifierToken)
                    : new RefExtensionNode(identifierToken, componentTagHelper.GetTypeName());

                node.Children.Add(refExtensionNode);
            }
        }

        private IntermediateToken DetermineIdentifierToken(ComponentAttributeExtensionNode attributeNode)
        {
            IntermediateToken foundToken = null;

            if (attributeNode.Children.Count == 1)
            {
                if (attributeNode.Children[0] is IntermediateToken token)
                {
                    foundToken = token;
                }
                else if (attributeNode.Children[0] is CSharpExpressionIntermediateNode csharpNode)
                {
                    if (csharpNode.Children.Count > 0)
                    {
                        foundToken = csharpNode.Children[0] as IntermediateToken;
                    }
                }
            }
            
            return !string.IsNullOrWhiteSpace(foundToken?.Content) ? foundToken : null;
        }
    }
}
