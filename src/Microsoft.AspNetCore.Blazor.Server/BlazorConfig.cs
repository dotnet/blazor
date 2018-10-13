// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;

namespace Microsoft.AspNetCore.Blazor.Server
{
    /// <summary>
    /// Represents configuration options for the blazor client. 
    /// </summary>
    public class BlazorConfig
    {
        /// <summary>
        /// Path to the blazor client project.
        /// </summary>
        public string SourceMSBuildPath { get; set; }

        /// <summary>
        /// Path to the blazor client assembly from within the client project.
        /// </summary>
        public string SourceOutputAssemblyPath { get; set; }

        /// <summary>
        /// Path to wwwroot folder from within the client project.
        /// </summary>
        public string WebRootPath { get; set; }

        /// <summary>
        /// Path to the compiled client distributables.
        /// </summary>
        public string DistPath { get; set; }

        /// <summary>
        /// Whether to enable client auto rebuilding.
        /// </summary>
        public bool EnableAutoRebuilding { get; set; }

        /// <summary>
        /// Whether to enable mono debugging.
        /// </summary>
        public bool EnableDebugging { get; set; }

        /// <summary>
        /// Loads the config options stored within the .blazor.config file stored next to the assembly file for the client.
        /// </summary>
        /// <param name="assemblyPath">path to client assembly within server bin directory</param>
        /// <returns></returns>
        public static BlazorConfig Read(string assemblyPath)
        {
            var config = new BlazorConfig();

            // TODO: Instead of assuming the lines are in a specific order, either JSON-encode
            // the whole thing, or at least give the lines key prefixes (e.g., "reload:<someuri>")
            // so we're not dependent on order and all lines being present.

            var configFilePath = Path.ChangeExtension(assemblyPath, ".blazor.config");
            var configLines = File.ReadLines(configFilePath).ToList();
            config.SourceMSBuildPath = configLines[0];

            if (config.SourceMSBuildPath == ".")
            {
                config.SourceMSBuildPath = assemblyPath;
            }

            var sourceMsBuildDir = Path.GetDirectoryName(config.SourceMSBuildPath);
            config.SourceOutputAssemblyPath = Path.Combine(sourceMsBuildDir, configLines[1]);
            config.DistPath = Path.Combine(Path.GetDirectoryName(config.SourceOutputAssemblyPath), "dist");

            var webRootPath = Path.Combine(sourceMsBuildDir, "wwwroot");
            if (Directory.Exists(webRootPath))
            {
                config.WebRootPath = webRootPath;
            }

            config.EnableAutoRebuilding = configLines.Contains("autorebuild:true", StringComparer.Ordinal);
            config.EnableDebugging = configLines.Contains("debug:true", StringComparer.Ordinal);

            return config;
        }
    }
}
