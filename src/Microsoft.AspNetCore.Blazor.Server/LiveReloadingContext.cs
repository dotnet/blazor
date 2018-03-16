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
    internal class LiveReloadingContext
    {
        // Keep in sync with $(BlazorBuildCompletedSignalPath) in Blazor.MonoRuntime.props
        private const string BlazorBuildCompletedSignalFile = "__blazorBuildCompleted";

        // If some external automated process is writing multiple files to wwwroot,
        // you probably want to wait until they've all been written before reloading.
        // Pausing by 500 milliseconds is a crude effort - we might need a different
        // mechanism (e.g., waiting until writes have stopped by 500ms).
        private const int WebRootUpdateDelayMilliseconds = 500;
        private const string heartbeatMessage = "data: alive\n\n";
        private const string reloadMessage = "data: reload\n\n";

        // If we don't hold references to them, then on Linux they get disposed.
        // TODO: Review if this is still true
        // This static would leak memory if you called UseBlazorLiveReloading continually
        // throughout the app lifetime, but the intended usage is just during init.
        private static readonly List<FileSystemWatcher> _pinnedWatchers = new List<FileSystemWatcher>();

        private readonly object _currentReloadListenerLock = new object();
        private CancellationTokenSource _currentReloadListener
            = new CancellationTokenSource();

        public void Attach(IApplicationBuilder applicationBuilder, BlazorConfig config)
        {
            CreateFileSystemWatchers(config);
            AddEventStreamEndpoint(applicationBuilder, config.ReloadUri);
        }

        private void AddEventStreamEndpoint(IApplicationBuilder applicationBuilder, string url)
        {
            applicationBuilder.Use(async (context, next) =>
            {
                if (!string.Equals(context.Request.Path, url, StringComparison.Ordinal))
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
                        await context.Response.Body.FlushAsync();
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
                                await context.Response.Body.FlushAsync();
                            }
                            break;
                        }
                    }
                }
            });
        }

        private void CreateFileSystemWatchers(BlazorConfig config)
        {
            // Watch for the "build completed" signal in the dist dir
            var distFileWatcher = new FileSystemWatcher(config.DistPath);
            distFileWatcher.Deleted += (sender, eventArgs) => {
                if (eventArgs.Name.Equals(BlazorBuildCompletedSignalFile, StringComparison.Ordinal))
                {
                    RequestReload(0);
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
                webRootWatcher.Deleted += (sender, evtArgs) => RequestReload(WebRootUpdateDelayMilliseconds);
                webRootWatcher.Created += (sender, evtArgs) => RequestReload(WebRootUpdateDelayMilliseconds);
                webRootWatcher.Changed += (sender, evtArgs) => RequestReload(WebRootUpdateDelayMilliseconds);
                webRootWatcher.Renamed += (sender, evtArgs) => RequestReload(WebRootUpdateDelayMilliseconds);
                webRootWatcher.EnableRaisingEvents = true;
                _pinnedWatchers.Add(webRootWatcher);
            }
        }

        private void RequestReload(int delayMilliseconds)
        {
            Task.Delay(delayMilliseconds).ContinueWith(_ =>
            {
                lock (_currentReloadListenerLock)
                {
                    // Lock just to be sure two threads don't assign different new CTSs, of which
                    // only one would later get cancelled.
                    _currentReloadListener.Cancel();
                    _currentReloadListener = new CancellationTokenSource();
                }
            });
        }
    }
}
