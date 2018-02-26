// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Microsoft.AspNetCore.Blazor.Razor
{
    public class ComponentDocumentClassifierPass : DocumentClassifierPassBase, IRazorDocumentClassifierPass
    {
        public static readonly string ComponentDocumentKind = "Blazor.Component-0.0.5";

        private static readonly char[] PathSeparators = new char[] { '/', '\\' };

        // This will be set by our CLI tool when running at the command line. The default value
        // will be used by the IDE.
        public string BaseNamespace { get; set; } = "HostedInAspNet.Client"; //"__BlazorGenerated";

        protected override string DocumentKind => ComponentDocumentKind;

        protected override bool IsMatch(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode)
        {
            // Treat everything as a component by default if Blazor is part of the configuration.
            return true;
        }

        protected override void OnDocumentStructureCreated(
            RazorCodeDocument codeDocument, 
            NamespaceDeclarationIntermediateNode @namespace, 
            ClassDeclarationIntermediateNode @class, 
            MethodDeclarationIntermediateNode method)
        {
            if (!TryComputeNamespaceAndClass(
                codeDocument.Source.RelativePath, 
                out var computedNamespace,
                out var computedClass))
            {
                // If we can't compute a nice namespace (no relative path) then just generate something
                // mangled.
                computedNamespace = BaseNamespace;
                computedClass = CSharpIdentifier.GetClassNameFromPath(codeDocument.Source.FilePath) ?? "__BlazorComponent";
            }

            @namespace.Content = computedNamespace;

            @class.BaseType = BlazorApi.BlazorComponent.FullTypeName;
            @class.ClassName = computedClass;
            @class.Modifiers.Clear();
            @class.Modifiers.Add("public");

            method.ReturnType = "void";
            method.MethodName = BlazorApi.BlazorComponent.BuildRenderTree;
            method.Modifiers.Clear();
            method.Modifiers.Add("protected");
            method.Modifiers.Add("override");

            method.Parameters.Clear();
            method.Parameters.Add(new MethodParameter()
            {
                ParameterName = "builder",
                TypeName = BlazorApi.RenderTreeBuilder.FullTypeName,
            });

            // We need to call the 'base' method as the first statement.
            var callBase = new CSharpCodeIntermediateNode();
            callBase.Children.Add(new IntermediateToken
            {
                Kind = TokenKind.CSharp,
                Content = $"base.{BlazorApi.BlazorComponent.BuildRenderTree}(builder);" + Environment.NewLine
            });
            method.Children.Insert(0, callBase);
        }

        // In general documents will have a relative path (relative to the project root).
        // We can only really compute a nice class/namespace when we know a relative path.
        //
        // However all kinds of thing are possible in tools. We shouldn't barf here if the document isn't 
        // set up correctly.
        private bool TryComputeNamespaceAndClass(string relativePath, out string @namespace, out string @class)
        {
            if (relativePath == null)
            {
                @namespace = null;
                @class = null;
                return false;
            }

            var builder = new StringBuilder();
            builder.Append(BaseNamespace); // Don't sanitize, we expect it to contain dots.

            var segments = relativePath.Split(PathSeparators);

            // Skip the last segment because it's the FileName.
            for (var i = 0; i < segments.Length - 1; i++)
            {
                builder.Append('.');
                builder.Append(CSharpIdentifier.SanitizeClassName(segments[i]));
            }

            @namespace = builder.ToString();
            @class = Path.GetFileNameWithoutExtension(relativePath);

            return true;
        }

        #region Workaround
        // This is a workaround for the fact that the base class doesn't provide good support
        // for replacing the IntermediateNodeWriter when building the code target. 
        void IRazorDocumentClassifierPass.Execute(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode)
        {
            base.Execute(codeDocument, documentNode);
            documentNode.Target = new BlazorCodeTarget(documentNode.Options, _targetExtensions);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            var feature = Engine.Features.OfType<IRazorTargetExtensionFeature>();
            _targetExtensions = feature.FirstOrDefault()?.TargetExtensions.ToArray() ?? EmptyExtensionArray;
        }

        private static readonly ICodeTargetExtension[] EmptyExtensionArray = new ICodeTargetExtension[0];
        private ICodeTargetExtension[] _targetExtensions;
        #endregion
    }
}
