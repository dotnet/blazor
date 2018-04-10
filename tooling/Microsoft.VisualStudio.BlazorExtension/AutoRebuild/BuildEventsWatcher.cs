// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.BlazorExtension
{
    internal class BuildEventsWatcher : IVsUpdateSolutionEvents2
    {
        // TODO: Also track "last build" timestamp for each project, and then amend
        // the protocol to "build project <path> if not built since <timestamp>"
        // where <timestamp> comes from the FSW change event (so that if you build
        // manually, you don't have to wait for a further rebuild)

        private object pendingBuildResultTasksLock = new object();
        private List<PendingBuildResultTask> pendingBuildResultTasks = new List<PendingBuildResultTask>();

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
           => VSConstants.S_OK;

        public int UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
        {
            var buildResult = fSuccess == 1;
            var ctx = (IVsBrowseObjectContext)pCfgProj;
            var projectPath = ctx.UnconfiguredProject.FullPath;
            foreach (var pendingBuildTask in GetAndRemovePendingBuildTasks(projectPath))
            {
                pendingBuildTask.TaskCompletionSource.SetResult(buildResult);
            }

            return VSConstants.S_OK;
        }

        private IEnumerable<PendingBuildResultTask> GetAndRemovePendingBuildTasks(string projectPath)
        {
            lock (pendingBuildResultTasksLock)
            {
                if (pendingBuildResultTasks.Count == 0)
                {
                    return Enumerable.Empty<PendingBuildResultTask>();
                }

                var result = new List<PendingBuildResultTask>();

                for (var index = 0; index < pendingBuildResultTasks.Count; index++)
                {
                    var candidate = pendingBuildResultTasks[index];
                    if (candidate.ProjectPath.Equals(projectPath, StringComparison.Ordinal))
                    {
                        result.Add(candidate);
                        pendingBuildResultTasks.RemoveAt(index);
                        index--;
                    }
                }

                return result;
            }
        }

        public Task<bool> GetNextBuildResultAsync(string projectPath)
        {
            var pendingBuildResultTask = new PendingBuildResultTask(projectPath);
            lock (pendingBuildResultTasksLock)
            {
                pendingBuildResultTasks.Add(pendingBuildResultTask);
            }
            return pendingBuildResultTask.TaskCompletionSource.Task;
        }

        public void DiscardPendingBuildTask(Task<bool> projectBuildTask)
        {
            lock (pendingBuildResultTasksLock)
            {
                pendingBuildResultTasks.RemoveAll(x => x.TaskCompletionSource.Task == projectBuildTask);
            }
        }

        class PendingBuildResultTask
        {
            public string ProjectPath { get; }
            public TaskCompletionSource<bool> TaskCompletionSource { get; }

            public PendingBuildResultTask(string projectPath)
            {
                ProjectPath = projectPath;
                TaskCompletionSource = new TaskCompletionSource<bool>();
            }
        }
    }
}
