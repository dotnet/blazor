// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Blazor.Server
{
    internal static class AutoRecompilation
    {
        // TODO: Make this configurable in csproj
        private static string[] _watchedExtensions = new[] { ".cs", ".cshtml" };
        private static string[] _excludeSubdirectories = new[] { "obj", "bin" };

        public static void UseAutoRecompilation(this IApplicationBuilder appBuilder, BlazorConfig config)
        {
            var currentCompilationTask = Task.CompletedTask;
            var compilationTaskMustBeRefreshed = false;

            WatchFileSystem(config, () =>
            {
                // Don't start the recompilation immediately. We only start it when the next
                // HTTP request arrives, because it's annoying if the IDE is constantly rebuilding
                // when you're making changes to multiple files and aren't ready to reload
                // in the browser yet.
                compilationTaskMustBeRefreshed = true;
            });

            appBuilder.Use(async (context, next) =>
            {
                if (compilationTaskMustBeRefreshed)
                {
                    currentCompilationTask = Recompile(config);
                    compilationTaskMustBeRefreshed = false;
                }

                await currentCompilationTask;
                await next();
            });
        }

        private static Task Recompile(BlazorConfig config)
        {
            var tcs = new TaskCompletionSource<object>();

            new Thread(() =>
            {
                // TODO: Perform the build inside VS instead of as a command-line build
                // TODO: Capture build errors/timeouts and show them in the browser
                var proc = Process.Start(new ProcessStartInfo
                {
                    WorkingDirectory = Path.GetDirectoryName(config.SourceMSBuildPath),
                    FileName = "dotnet",
                    Arguments = "build --no-restore --no-dependencies " + Path.GetFileName(config.SourceMSBuildPath),
                });
                proc.WaitForExit(60 * 1000);
                tcs.SetResult(new object());
            }).Start();

            return tcs.Task;
        }

        private static void WatchFileSystem(BlazorConfig config, Action onWrite)
        {
            var clientAppRootDir = Path.GetDirectoryName(config.SourceMSBuildPath);
            var excludePathPrefixes = _excludeSubdirectories.Select(subdir
                => Path.Combine(clientAppRootDir, subdir) + Path.DirectorySeparatorChar);

            var fsw = new FileSystemWatcher(clientAppRootDir);
            fsw.Created += OnEvent;
            fsw.Changed += OnEvent;
            fsw.Deleted += OnEvent;
            fsw.Renamed += OnEvent;
            fsw.IncludeSubdirectories = true;
            fsw.EnableRaisingEvents = true;

            void OnEvent(object sender, FileSystemEventArgs eventArgs)
            {
                if (!File.Exists(eventArgs.FullPath))
                {
                    // It's probably a directory rather than a file
                    return;
                }

                if (!_watchedExtensions.Any(ext => eventArgs.Name.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                {
                    // Not a candiate file type
                    return;
                }

                if (excludePathPrefixes.Any(prefix => eventArgs.FullPath.StartsWith(prefix, StringComparison.Ordinal)))
                {
                    // In an excluded subdirectory
                    return;
                }

                onWrite();
            }
        }
    }
}
