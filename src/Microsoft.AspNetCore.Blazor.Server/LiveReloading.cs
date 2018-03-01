// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Blazor.Server
{
    internal static class LiveReloading
    {
        // Keep in sync with the const in Microsoft.AspNetCore.Blazor.Build's AppBuilder.cs
        const string BlazorBuildCompletedSignalFile = "__blazorBuildCompleted";

        // If we don't hold references to them, then on Linux they get disposed
        // TODO: Review if this is still true
        readonly static List<FileSystemWatcher> _pinnedWatchers = new List<FileSystemWatcher>();

        readonly static string heartbeatMessage = $"data: alive\n\n";
        readonly static string reloadMessage = $"data: reload\n\n";
        readonly static object _currentReloadListenerLock = new object();
        static CancellationTokenSource _currentReloadListener
            = new CancellationTokenSource();

        public static void UseBlazorLiveReloading(
            this IApplicationBuilder applicationBuilder,
            BlazorConfig config)
        {
            if (!string.IsNullOrEmpty(config.ReloadUri))
            {
                CreateFileSystemWatchers(config);
                AddEventStreamEndpoint(applicationBuilder, config.ReloadUri);
            }
        }

        private static void AddEventStreamEndpoint(IApplicationBuilder applicationBuilder, string url)
        {
            applicationBuilder.Use(async (context, next) =>
            {
                if (context.Request.Path != url)
                {
                    await next();
                }
                else
                {
                    context.Response.ContentType = "text/event-stream";
                    var reloadToken = _currentReloadListener.Token;
                    var reloadOrRequestAbortedToken = CancellationTokenSource
                        .CreateLinkedTokenSource(reloadToken, context.RequestAborted)
                        .Token;
                    while (!context.RequestAborted.IsCancellationRequested)
                    {
                        await context.Response.WriteAsync(heartbeatMessage);
                        await context.Response.WriteAsync(Environment.NewLine);
                        try
                        {
                            await Task.Delay(5000, reloadOrRequestAbortedToken);
                        }
                        catch (TaskCanceledException)
                        {
                            if (reloadToken.IsCancellationRequested)
                            {
                                await context.Response.WriteAsync(reloadMessage);
                                await context.Response.WriteAsync(Environment.NewLine);
                            }
                            break;
                        }
                    }
                }
            });
        }

        private static void CreateFileSystemWatchers(BlazorConfig config)
        {
            // Watch for the "build completed" signal in the dist dir
            var distFileWatcher = new FileSystemWatcher(config.DistPath);
            distFileWatcher.Deleted += (sender, eventArgs) => {
                if (eventArgs.Name.Equals(BlazorBuildCompletedSignalFile, StringComparison.Ordinal))
                {
                    RequestReload();
                }
            };
            distFileWatcher.EnableRaisingEvents = true;
            _pinnedWatchers.Add(distFileWatcher);

            // If there's a WebRootPath, watch for any file modification there
            // WebRootPath is only used in dev builds, where we want to serve from wwwroot directly
            // without requiring the developer to rebuild after changing the static files there.
            // In production there's no need for it.
            if (!string.IsNullOrEmpty(config.WebRootPath))
            {
                var webRootWatcher = new FileSystemWatcher(config.WebRootPath);
                webRootWatcher.Deleted += (sender, evtArgs) => RequestReload();
                webRootWatcher.Created += (sender, evtArgs) => RequestReload();
                webRootWatcher.Changed += (sender, evtArgs) => RequestReload();
                webRootWatcher.Renamed += (sender, evtArgs) => RequestReload();
                webRootWatcher.EnableRaisingEvents = true;
                _pinnedWatchers.Add(webRootWatcher);
            }
        }

        private static void RequestReload()
        {
            lock (_currentReloadListenerLock)
            {
                // Lock just to be sure two threads don't assign different new CTSs, of which
                // only one would later get cancelled.
                _currentReloadListener.Cancel();
                _currentReloadListener = new CancellationTokenSource();
            }
        }
    }
}
