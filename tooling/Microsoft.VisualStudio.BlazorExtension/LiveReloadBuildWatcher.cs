// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.VisualStudio.BlazorExtension
{
    internal class LiveReloadBuildWatcher : IVsUpdateSolutionEvents2
    {
        const string BlazorProjectCapability = "Blazor";

        private bool _isListeningForProjectBuilds = false;
        private List<string> _signalFilePathsToNotify = new List<string>();

        public int UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            _signalFilePathsToNotify.Clear();
            _isListeningForProjectBuilds = true;
            return VSConstants.S_OK;
        }

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            _isListeningForProjectBuilds = false;

            foreach (var fullPath in _signalFilePathsToNotify)
            {
                File.WriteAllText(fullPath, string.Empty);
                File.Delete(fullPath);
            }

            return VSConstants.S_OK;
        }

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
            if (_isListeningForProjectBuilds
                && fSuccess == 1 // i.e., build succeeded
                && pHierProj.IsCapabilityMatch(BlazorProjectCapability))
            {
                var configuredProject = ((IVsBrowseObjectContext)pCfgProj).ConfiguredProject;
                var projectLockService = configuredProject
                    .UnconfiguredProject
                    .ProjectService
                    .Services
                    .ProjectLockService;

                ThreadHelper.JoinableTaskFactory.Run(async delegate
                {
                    using (var access = await projectLockService.ReadLockAsync())
                    {
                        var project = await access.GetProjectAsync(configuredProject);

                        // Now we can evaluate MSBuild properties
                        var signalFileFullPath = project.GetPropertyValue("BlazorBuildCompletedSignalFullPath");
                        if (!string.IsNullOrEmpty(signalFileFullPath))
                        {
                            _signalFilePathsToNotify.Add(signalFileFullPath);
                        }
                    }
                });
            }

            return VSConstants.S_OK;
        }
    }
}
