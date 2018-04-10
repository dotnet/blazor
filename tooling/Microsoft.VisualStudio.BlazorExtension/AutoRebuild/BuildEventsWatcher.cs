// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.BlazorExtension
{
    /// <summary>
    /// Watches for Blazor project build events, starts new builds, and tracks builds in progress.
    /// </summary>
    internal class BuildEventsWatcher : IVsUpdateSolutionEvents2
    {
        private readonly IVsSolution _vsSolution;
        private readonly IVsSolutionBuildManager _vsBuildManager;
        private readonly object mostRecentBuildInfosLock = new object();
        private readonly Dictionary<string, BuildInfo> mostRecentBuildInfos = new Dictionary<string, BuildInfo>();

        public BuildEventsWatcher(IVsSolution vsSolution, IVsSolutionBuildManager vsBuildManager)
        {
            _vsSolution = vsSolution ?? throw new ArgumentNullException(nameof(vsSolution));
            _vsBuildManager = vsBuildManager ?? throw new ArgumentNullException(nameof(vsBuildManager));
        }

        public int UpdateSolution_Begin(ref int pfCancelUpdate)
           => VSConstants.S_OK;

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
           => VSConstants.S_OK;

        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
           => VSConstants.S_OK;

        public int UpdateSolution_Cancel()
           => VSConstants.S_OK;

        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
           => VSConstants.S_OK;

        public int UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel)
        {
            if (IsBlazorProject(pHierProj))
            {
                // If there isn't an in-progress BuildInfo for this project (either there's no record,
                // or the previous build already finished), then create one. So if there's a manually-
                // triggered build and then a build request arrives while it's still in progress, we'll
                // use the existing build rather than waiting for a subsequent one.
                var ctx = (IVsBrowseObjectContext)pCfgProj;
                var projectPath = ctx.UnconfiguredProject.FullPath;
                lock (mostRecentBuildInfosLock)
                {
                    var hasBuildInProgress =
                        mostRecentBuildInfos.TryGetValue(projectPath, out var existingInfo)
                        && !existingInfo.TaskCompletionSource.Task.IsCompleted;
                    if (!hasBuildInProgress)
                    {
                        mostRecentBuildInfos[projectPath] = new BuildInfo();
                    }
                }
            }

            return VSConstants.S_OK;
        }


        public int UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
        {
            if (IsBlazorProject(pHierProj))
            {
                var buildResult = fSuccess == 1;
                var ctx = (IVsBrowseObjectContext)pCfgProj;
                var projectPath = ctx.UnconfiguredProject.FullPath;
                MarkPendingBuildTasksCompleted(projectPath, buildResult);
            }

            return VSConstants.S_OK;
        }

        public Task<bool> PerformBuildAsync(string projectPath, DateTime allowExistingBuildsSince)
        {
            BuildInfo newBuildInfo;

            lock (mostRecentBuildInfosLock)
            {
                if (mostRecentBuildInfos.TryGetValue(projectPath, out var existingInfo))
                {
                    var acceptBuild = !existingInfo.TaskCompletionSource.Task.IsCompleted
                        || existingInfo.StartTime > allowExistingBuildsSince;
                    if (acceptBuild)
                    {
                        return existingInfo.TaskCompletionSource.Task;
                    }
                }
                
                mostRecentBuildInfos[projectPath] = newBuildInfo = new BuildInfo();
            }

            return PerformNewBuildAsync(projectPath, newBuildInfo.TaskCompletionSource.Task);
        }

        private async Task<bool> PerformNewBuildAsync(string projectPath, Task<bool> buildInProgressTask)
        {
            // Switch to the UI thread and request the build
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

            return didStartBuild && await buildInProgressTask;
        }

        private void MarkPendingBuildTasksCompleted(string projectPath, bool buildResult)
        {
            BuildInfo foundInfo = null;
            lock (mostRecentBuildInfosLock)
            {
                mostRecentBuildInfos.TryGetValue(projectPath, out foundInfo);
            }

            if (foundInfo != null)
            {
                foundInfo.TaskCompletionSource.TrySetResult(buildResult);
            }
        }

        private static bool IsBlazorProject(IVsHierarchy pHierProj)
            => pHierProj.IsCapabilityMatch("Blazor");

        class BuildInfo
        {
            public DateTime StartTime { get; }
            public TaskCompletionSource<bool> TaskCompletionSource { get; }

            public BuildInfo()
            {
                StartTime = DateTime.Now;
                TaskCompletionSource = new TaskCompletionSource<bool>();
            }
        }
    }
}
