// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Blazor.Server.AutoRebuild
{
    /// <summary>
    /// Finds the VS process that launched this app process (if any), and uses
    /// named pipes to communicate with its AutoRebuild listener (if any).
    /// </summary>
    internal class VSForWindowsRebuildService : IRebuildService
    {
        private const int _connectionTimeoutMilliseconds = 3000;
        private readonly Process _vsProcess;

        public static bool TryCreate(out VSForWindowsRebuildService result)
        {
            var vsProcess = FindAncestorVSProcess();
            if (vsProcess != null)
            {
                result = new VSForWindowsRebuildService(vsProcess);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public async Task<bool> PerformRebuildAsync(string projectFullPath, DateTime ifNotBuiltSince)
        {
            var pipeName = $"BlazorAutoRebuild\\{_vsProcess.Id}";
            using (var pipeClient = new NamedPipeClientStream(pipeName))
            {
                await pipeClient.ConnectAsync(_connectionTimeoutMilliseconds);

                // Protocol:
                //   1. Receive protocol version number from the VS listener
                //      If we're incompatible with it, send back special string "abort" and end
                //   2. Send the project path to the VS listener
                //   3. Send the 'if not rebuilt since' timestamp to the VS listener
                //   4. Wait for it to send back a bool representing the result
                // Keep in sync with AutoRebuildService.cs in the BlazorExtension project
                // In the future we may extend this to getting back build error details
                var remoteProtocolVersion = await pipeClient.ReadIntAsync();
                if (remoteProtocolVersion == 1)
                {
                    await pipeClient.WriteStringAsync(projectFullPath);
                    await pipeClient.WriteDateTimeAsync(ifNotBuiltSince);
                    return await pipeClient.ReadBoolAsync();
                }
                else
                {
                    await pipeClient.WriteStringAsync("abort");
                    return false;
                }
            }
        }

        private VSForWindowsRebuildService(Process vsProcess)
        {
            _vsProcess = vsProcess ?? throw new ArgumentNullException(nameof(vsProcess));
        }

        private static Process FindIISHostedVSProcess(Process VSIISExeLauncher)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return null;
            }

            var launcherDirectory = Path.GetDirectoryName(VSIISExeLauncher.MainModule.FileName);

            //Assuming default IISLauncherArgs.txt filename
            var IISExeLauncherArgs = Path.Combine(launcherDirectory, "IISExeLauncherArgs.txt");
            if (!File.Exists(IISExeLauncherArgs))
                return null;

            string args = File.ReadAllText(IISExeLauncherArgs);
            Match m = Regex.Match(args, @"-owningPid (?<Pid>[0-9]+)");

            int owningPid = 0;
            if (!int.TryParse(m.Groups["Pid"]?.Value, out owningPid))
            {
                return null;
            }

            try
            {
                var vsProcess = Process.GetProcessById(owningPid);
                if (vsProcess != null && vsProcess.ProcessName.Equals("devenv", StringComparison.OrdinalIgnoreCase) && !vsProcess.HasExited)
                    return vsProcess;
            }
            catch (Exception)
            {
                // There's probably some permissions issue that prevents us from seeing
                // more information about devenv process. The used service account in IIS
                // may not be in the administrator group, and lack of privilege.
            }

            //If no process were found or if the devenv process exited, then there is no eligible PID
            return null;
        }

        private static Process FindAncestorVSProcess()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return null;
            }

            var candidateProcess = Process.GetCurrentProcess();
            try
            {
                while (candidateProcess != null && !candidateProcess.HasExited)
                {
                    // It's unlikely that anyone's going to have a non-VS process in the process
                    // hierarchy called 'devenv', but if that turns out to be a scenario, we could
                    // (for example) write the VS PID to the obj directory during build, and then
                    // only consider processes with that ID. We still want to be sure there really
                    // is such a process in our ancestor chain, otherwise if you did "dotnet run"
                    // in a command prompt, we'd be confused and think it was launched from VS.
                    if (candidateProcess.ProcessName.Equals("devenv", StringComparison.OrdinalIgnoreCase))
                    {
                        return candidateProcess;
                    }

                    // The currently debuggable process may be launched from the VSIISExeLauncher program
                    // if hosted in IIS with Visual Studio IIS continuous integration. In this scenario we
                    // retrieve VS PID (devenv) from IISExeLauncherArgs.txt file, in owningPid option.
                    if (candidateProcess.ProcessName.Equals("VSIISExeLauncher", StringComparison.OrdinalIgnoreCase))
                    {
                        var hostedVSProcess = FindIISHostedVSProcess(candidateProcess);
                        if (hostedVSProcess != null)
                        {
                            return hostedVSProcess;
                        }
                    }

                    candidateProcess = ProcessUtils.GetParent(candidateProcess);
                }
            }
            catch (Exception)
            {
                // There's probably some permissions issue that prevents us from seeing
                // further up the ancestor list, so we have to stop looking here.
            }

            return null;
        }
    }
}
