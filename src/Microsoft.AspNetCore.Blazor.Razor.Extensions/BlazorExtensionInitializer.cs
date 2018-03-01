// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;

namespace Microsoft.AspNetCore.Blazor.Razor
{
    public class BlazorExtensionInitializer : RazorExtensionInitializer
    {
        public static readonly RazorConfiguration DeclarationConfiguration = new RazorConfiguration(
            RazorLanguageVersion.Version_2_1,
            "BlazorDeclaration-0.0.5",
            Array.Empty<RazorExtension>());

        public static readonly RazorConfiguration DefaultConfiguration = new RazorConfiguration(
            RazorLanguageVersion.Version_2_1,
            "Blazor-0.0.5",
            Array.Empty<RazorExtension>());

        public static void Register(RazorProjectEngineBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            FunctionsDirective.Register(builder);
            ImplementsDirective.Register(builder);
            InheritsDirective.Register(builder);
            InjectDirective.Register(builder);
            LayoutDirective.Register(builder);

            builder.Features.Remove(builder.Features.OfType<IImportProjectFeature>().Single());
            builder.Features.Add(new BlazorImportProjectFeature());

            var index = builder.Phases.IndexOf(builder.Phases.OfType<IRazorCSharpLoweringPhase>().Single());
            builder.Phases[index] = new BlazorRazorCSharpLoweringPhase();

            builder.Features.Add(new ConfigureBlazorCodeGenerationOptions());

            builder.Features.Add(new ComponentDocumentClassifierPass());

            builder.Features.Add(new ComponentTagHelperDescriptorProvider());

            if (builder.Configuration.ConfigurationName == DeclarationConfiguration.ConfigurationName)
            {
                // This is for 'declaration only' processing. We don't want to try and emit any method bodies during
                // the design time build because we can't do it correctly until the set of components is known.
                builder.Features.Add(new EliminateMethodBodyPass());
            }
        }

        // This is temporarily used to initialize a RazorEngine by the build tools until we get the features
        // we need into the RazorProjectEngine (namespace).
        public static void Register(IRazorEngineBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            FunctionsDirective.Register(builder);
            ImplementsDirective.Register(builder);
            InheritsDirective.Register(builder);
            InjectDirective.Register(builder);
            LayoutDirective.Register(builder);

            builder.Features.Add(new ConfigureBlazorCodeGenerationOptions());

            builder.Features.Add(new ComponentDocumentClassifierPass());

            builder.Features.Add(new ComponentTagHelperDescriptorProvider());
        }

        public override void Initialize(RazorProjectEngineBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            Register(builder);
        }

        private class ConfigureBlazorCodeGenerationOptions : IConfigureRazorCodeGenerationOptionsFeature
        {
            public int Order => 0;

            public RazorEngine Engine { get; set; }

            public void Configure(RazorCodeGenerationOptionsBuilder options)
            {
                if (options == null)
                {
                    throw new ArgumentNullException(nameof(options));
                }

                // These metadata attributes require a reference to the Razor.Runtime package which we don't
                // otherwise need.
                options.SuppressMetadataAttributes = true;
            }
        }
    }
}
