// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Package = Microsoft.VisualStudio.Shell.Package;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace Microsoft.VisualStudio.BlazorExtension
{
    /// <summary>
    /// The counterpart to VSForWindowsRebuildService.cs in the Blazor.Server project.
    /// Listens for named pipe connections and rebuilds projects on request.
    /// </summary>
    internal class AutoRebuildService
    {
        private readonly IVsSolution _vsSolution;
        private readonly IVsSolutionBuildManager _vsBuildManager;
        private readonly BuildEventsWatcher _buildEventsWatcher;
        private readonly string _pipeName;

        public AutoRebuildService(
            IVsSolution vsSolution,
            IVsSolutionBuildManager vsBuildManager,
            BuildEventsWatcher buildEventsWatcher)
        {
            _vsSolution = vsSolution ?? throw new ArgumentNullException(nameof(vsSolution));
            _vsBuildManager = vsBuildManager ?? throw new ArgumentNullException(nameof(vsBuildManager));
            _buildEventsWatcher = buildEventsWatcher ?? throw new ArgumentNullException(nameof(buildEventsWatcher));
            _pipeName = $"BlazorAutoRebuild\\{Process.GetCurrentProcess().Id}";
        }

        public void Listen()
        {
            AddBuildServiceNamedPipeServer();
        }

        private void AddBuildServiceNamedPipeServer()
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    using (var serverPipe = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances))
                    {
                        // As soon as we receive a connection, spin up another background
                        // listener to wait for the next connection
                        await serverPipe.WaitForConnectionAsync();
                        AddBuildServiceNamedPipeServer();

                        await HandleRequestAsync(serverPipe);
                    }
                }
                catch (Exception ex)
                {
                    await AttemptLogErrorAsync(
                        $"Error in Blazor AutoRebuildService:\n{ex.Message}\n{ex.StackTrace}");
                }
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private async Task HandleRequestAsync(Stream stream)
        {
            // Very simple protocol:
            //   1. Receive the project path from the server instance
            //   2. Perform the build, then send back the success/failure result flag
            // Keep in sync with VSForWindowsRebuildService.cs in the Blazor.Server project
            // In the future we may extend this to getting back build error details
            var projectPath = await stream.ReadStringAsync();
            var buildResult = await PerformBuildAsync(projectPath);
            await stream.WriteBoolAsync(buildResult);
        }

        private async Task<bool> PerformBuildAsync(string projectPath)
        {
            // First get a task that represents the next build completion for this project.
            // We do this before asking for the build in case the build completes instantly.
            var projectBuildTask = _buildEventsWatcher.GetNextBuildResultAsync(projectPath);

            // Now switch to the UI thread and request the build
            var didStartBuild = await ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var hr = _vsSolution.GetProjectOfUniqueName(projectPath, out var hierarchy);
                if (hr != VSConstants.S_OK)
                {
                    return false;
                }

                hr = _vsBuildManager.StartSimpleUpdateProjectConfiguration(
                    hierarchy,
                    /* not used */ null,
                    /* not used */ null,
                    (uint)VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD,
                    /* other flags */ 0,
                    /* suppress dialogs */ 1);
                if (hr != VSConstants.S_OK)
                {
                    return false;
                }

                return true;
            });

            // If we didn't manage to start a build, then there's no point continuing to
            // hold the pending build task in memory
            if (!didStartBuild)
            {
                _buildEventsWatcher.DiscardPendingBuildTask(projectBuildTask);
            }

            return didStartBuild && await projectBuildTask;
        }

        private async Task AttemptLogErrorAsync(string message)
        {
            if (!ThreadHelper.CheckAccess())
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            }

            var outputWindow = (IVsOutputWindow)Package.GetGlobalService(typeof(SVsOutputWindow));
            if (outputWindow != null)
            {
                outputWindow.GetPane(VSConstants.OutputWindowPaneGuid.BuildOutputPane_guid, out var pane);
                if (pane != null)
                {
                    pane.OutputString(message);
                    pane.Activate();
                }
            }
        }
    }
}
