// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Blazor.Build.Core.FileSystem;

namespace Microsoft.AspNetCore.Blazor.Build.Core
{
    internal static class AppBuilder
    {
        internal static void Execute(string assemblyPath, string webRootPath)
        {
            var outputPath = Path.GetDirectoryName(assemblyPath);
            if (!File.Exists(assemblyPath))
            {
                throw new FileNotFoundException($"Could not find client assembly file at '{assemblyPath}'.", assemblyPath);
            }
            var frameworkFileProvider = new FrameworkFileProvider(assemblyPath);

            var tmpDistDirPath = Path.Combine(outputPath, "tmp");
            var tmpOptimizedPath = Path.Combine(outputPath, "optimized");

            FileUtil.WriteFileProviderToDisk(frameworkFileProvider, Path.Combine(outputPath, "manual"), clean: true);

            // Pull all the files on a temporary folder to run the linker on.
            FileUtil.WriteFileProviderToDisk(frameworkFileProvider, Path.Combine(tmpDistDirPath, "_framework"), clean: true);
            RunLinker(assemblyPath, tmpDistDirPath, tmpOptimizedPath);
            var distDirPath = Path.Combine(outputPath, "dist");

            // Pull all the files into the dist folder
            FileUtil.WriteFileProviderToDisk(frameworkFileProvider, Path.Combine(distDirPath, "_framework"), clean: true);

            // Replace the files in the bin folder with the optimized files
            var distFrameworkBin = Path.Combine(distDirPath, "_framework", "_bin");

            foreach (var entry in Directory.GetFileSystemEntries(distFrameworkBin))
            {
                File.Delete(entry);
            }

            foreach (var entry in Directory.GetFileSystemEntries(tmpOptimizedPath))
            {
                File.Move(entry, Path.Combine(distFrameworkBin, Path.GetFileName(entry)));
            }

            //File.Copy(assemblyPath, Path.Combine(distFrameworkBin, Path.GetFileName(assemblyPath)), overwrite: true);

            // Write an updated index.html file if present.
            if (!string.IsNullOrEmpty(webRootPath))
            {
                var path = Path.Combine(webRootPath, "index.html");
                if (File.Exists(path))
                {
                    var indexOutputPath = Path.Combine(distDirPath, "index.html");
                    var referenceNames = Directory.EnumerateFileSystemEntries(distFrameworkBin)
                    .Select(file => Path.GetFileName(file))
                    .Where(file => !string.Equals(file, Path.GetFileName(assemblyPath)))
                    .ToArray();

                    IndexHtmlWriter.UpdateIndex(path, assemblyPath, referenceNames, indexOutputPath);
                }
            }

            // Cleanup
            Directory.Delete(tmpDistDirPath, recursive: true);
            Directory.Delete(tmpOptimizedPath, recursive: true);
        }

        private static void RunLinker(string assemblyPath, string srcPath, string outputPath)
        {
            EnsureDirectory(outputPath);
            var arguments = BuildArguments(assemblyPath, srcPath, outputPath);
            var processInfo = new ProcessStartInfo(
                @"dotnet",
                arguments)
            {
                RedirectStandardOutput = true
            };
            using (var proccess = Process.Start(processInfo))
            {
                Console.WriteLine("Running illink.exe with arguments:");
                Console.WriteLine(arguments);
                Console.WriteLine(proccess.StandardOutput.ReadToEnd());
                proccess.WaitForExit();
            }
        }

        private static string BuildArguments(string assemblyPath, string srcPath, string outputPath)
        {
            string[] mainAssemblies = GetMainAssemblies(assemblyPath);
            var frameworkFolder = Path.Combine(srcPath, "_framework", "_bin");
            var additionalOptions = $"-c link -u skip --skip-unresolved true";
            return @"C:\work\Blazor\src\mono\tools\bin\illink\illink.dll" +
                " " + additionalOptions +
                $" {$"-d {frameworkFolder}"}" +
                $" {$"-out {outputPath}"}" +
                $" {string.Join(' ', mainAssemblies.Select(s => $"-a {s}"))}";
        }

        private static string[] GetMainAssemblies(string assemblyPath)
        {
            var basePath = Path.GetDirectoryName(assemblyPath);
            var mainAssemblies = new[] {
                assemblyPath,
                Path.Combine(basePath, "Microsoft.AspNetCore.Blazor.dll"),
                Path.Combine(basePath, "Microsoft.AspNetCore.Blazor.Browser.dll")
            };
            return mainAssemblies;
        }

        private static void EnsureDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }
    }
}
