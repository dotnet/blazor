// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Blazor.Server.Rendering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace PrerenderingApp.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var directory = Directory.GetCurrentDirectory();
            var currentDirectory = Environment.CurrentDirectory;
            var codeBase = Assembly.GetEntryAssembly().CodeBase;
            var location = Assembly.GetEntryAssembly().Location;
            AssemblyLoadContext.Default.Resolving += Default_Resolving;

            /*var prog = typeof(Client.Program);
            var method = prog.GetMethod("Server", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            method.Invoke(null, new object[] { null });*/

            var renderer = new PreRenderer();//.AddComponent<Home>("app");
            renderer.DoStuff<Client.Home>();

            BuildWebHost(args).Run();
        }

        private static System.Reflection.Assembly Default_Resolving(AssemblyLoadContext arg1, System.Reflection.AssemblyName arg2)
        {
            var location = Assembly.GetEntryAssembly().Location;
            var directoryName = Path.GetDirectoryName(location);
            var assemblyPath = Path.Combine(directoryName, arg2.Name + ".dll");
            return arg1.LoadFromAssemblyPath(assemblyPath);
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
