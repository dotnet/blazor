// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Razor;

namespace Microsoft.AspNetCore.Blazor.Razor
{
    internal class ComponentTagHelperDescriptorProvider : RazorEngineFeatureBase, ITagHelperDescriptorProvider
    {
        public int Order { get; set; }

        public void Execute(TagHelperDescriptorProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var compilation = context.GetCompilation();
            if (compilation == null)
            {
                // No compilation, nothing to do.
                return;
            }

            var types = new List<INamedTypeSymbol>();
            var visitor = TagHelperTypeVisitor.Create(compilation, types);

            // We always visit the global namespace.
            visitor.Visit(compilation.Assembly.GlobalNamespace);

            foreach (var reference in compilation.References)
            {
                if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol assembly)
                {
                    if (IsTagHelperAssembly(assembly))
                    {
                        visitor.Visit(assembly.GlobalNamespace);
                    }
                }
            }

            var factory = new DefaultTagHelperDescriptorFactory(compilation, DesignTime);
            for (var i = 0; i < types.Count; i++)
            {
                var descriptor = factory.CreateDescriptor(types[i]);

                if (descriptor != null)
                {
                    context.Results.Add(descriptor);
                }
            }
        }

        private bool IsTagHelperAssembly(IAssemblySymbol assembly)
        {
            return assembly.Name != null && !assembly.Name.StartsWith("System.", StringComparison.Ordinal);
        }

        internal class ComponentTypeVisitor : SymbolVisitor
        {
            private INamedTypeSymbol _interface;
            private List<INamedTypeSymbol> _results;

            public static ComponentTypeVisitor Create(Compilation compilation, List<INamedTypeSymbol> results)
            {
                var @interface = compilation.GetTypeByMetadataName(BlazorApi..ITagHelper);
                return new ComponentTypeVisitor(@interface, results);
            }

            public TagHelperTypeVisitor(INamedTypeSymbol @interface, List<INamedTypeSymbol> results)
            {
                _interface = @interface;
                _results = results;
            }

            public override void VisitNamedType(INamedTypeSymbol symbol)
            {
                if (IsTagHelper(symbol))
                {
                    _results.Add(symbol);
                }
            }

            public override void VisitNamespace(INamespaceSymbol symbol)
            {
                foreach (var member in symbol.GetMembers())
                {
                    Visit(member);
                }
            }

            internal bool IsTagHelper(INamedTypeSymbol symbol)
            {
                if (_interface == null)
                {
                    return false;
                }

                return
                    symbol.DeclaredAccessibility == Accessibility.Public &&
                    !symbol.IsAbstract &&
                    !symbol.IsGenericType &&
                    symbol.AllInterfaces.Contains(_interface);
            }
        }
    }
}
}