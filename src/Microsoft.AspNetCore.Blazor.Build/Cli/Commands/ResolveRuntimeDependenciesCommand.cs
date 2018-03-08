// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Blazor.Build.Core;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.AspNetCore.Blazor.Build.Cli.Commands
{
    class ResolveRuntimeDependenciesCommand
    {
        public static void Command(CommandLineApplication command)
        {
            var references = command.Option("--reference",
                "Full path to the application dependencies that are not part of netstandard2.0",
                CommandOptionType.MultipleValue);

            var baseClassLibrary = command.Option("--base-class-library",
                "Full path to the directories with the dlls that make up the mono BCL",
                CommandOptionType.MultipleValue);

            var resolutionOverrides = command.Option(
                "--resolution-override",
                "Semi-colon separated value pair of assembly name, path",
                CommandOptionType.MultipleValue);

            var outputPath = command.Option("--output",
                "Path to the output file that will contain the selected assemblies",
                CommandOptionType.SingleValue);

            var mainAssemblyPath = command.Argument("assembly",
                "Path to the assembly containing the entry point of the application.");

            command.OnExecute(() =>
            {
                if (string.IsNullOrEmpty(mainAssemblyPath.Value) ||
                    !baseClassLibrary.HasValue() || !outputPath.HasValue())
                {
                    command.ShowHelp(command.Name);
                    return 1;
                }

                try
                {
                    RuntimeDependenciesResolver.ResolveRuntimeDependencies(
                        mainAssemblyPath.Value,
                        references.Values.ToArray(),
                        baseClassLibrary.Values.ToArray(),
                        resolutionOverrides.Values.ToArray(),
                        outputPath.Value());

                    return 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                    return 1;
                }
            });
        }
    }
}
