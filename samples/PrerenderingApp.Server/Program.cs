// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace PrerenderingApp.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AssemblyLoadContext.Default.Resolving += Default_Resolving;
            BuildWebHost(args).Run();
        }

        private static Assembly Default_Resolving(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            var location = Assembly.GetEntryAssembly().Location;
            var directoryName = Path.GetDirectoryName(location);
            var assemblyPath = Path.Combine(directoryName, assemblyName.Name + ".dll");
            return context.LoadFromAssemblyPath(assemblyPath);
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(new ConfigurationBuilder()
                    .AddCommandLine(args)
                    .Build())
                .UseStartup<Startup>()
                .Build();
    }
}
