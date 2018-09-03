// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.AspNetCore.Blazor.BuildTools.Core.ILWipe;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.AspNetCore.Blazor.BuildTools.Cli.Commands
{
    static class ILWipeCommand
    {
        public static void Command(CommandLineApplication command)
        {
            command.Description = "Wipes code from the specified assembly.";
            command.HelpOption("-?|-h|--help");

            var assemblyOption = command.Option(
                "-a|--assembly",
                "The assembly from which code should be wiped.",
                CommandOptionType.SingleValue);

            var specFileOption = command.Option(
                "-s|--spec",
                "The spec file describing which members to wipe from the assembly.",
                CommandOptionType.SingleValue);

            var verboseOption = command.Option(
                "-v|--verbose",
                "If set, logs additional information to the console.",
                CommandOptionType.NoValue);

            var listOption = command.Option(
                "-l|--list",
                "If set, just lists the assembly contents and does not write any other output to disk.",
                CommandOptionType.NoValue);

            var outputOption = command.Option(
                "-o|--output",
                "The location where the wiped assembly should be written.",
                CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                var inputPath = GetRequiredOptionValue(assemblyOption);

                if (listOption.HasValue())
                {
                    foreach (var item in AssemblyItem.ListContents(inputPath))
                    {
                        Console.WriteLine($"{item} {item.CodeSize}");
                    }
                }
                else
                {
                    var specLines = File.ReadAllLines(GetRequiredOptionValue(specFileOption));

                    var outputPath = WipeAssembly.Exec(
                        inputPath,
                        outputOption.Value(),
                        specLines,
                        verboseOption.HasValue());

                    Console.WriteLine(
                        $" Input: {inputPath} ({FormatFileSize(inputPath)})");
                    Console.WriteLine(
                        $"Output: {outputPath} ({FormatFileSize(outputPath)})");
                }

                return 0;
            });
        }

        private static string FormatFileSize(string path)
        {
            return FormatFileSize(new FileInfo(path).Length);
        }

        private static string FormatFileSize(long length)
        {
            return string.Format("{0:0.##} MB", ((double)length) / (1024*1024));
        }

        private static string GetRequiredOptionValue(CommandOption option)
        {
            if (!option.HasValue())
            {
                throw new InvalidOperationException($"Missing value for required option '{option.LongName}'.");
            }

            return option.Value();
        }
    }
}
