// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Microsoft.AspNetCore.Blazor.Cli.Server
{
    internal class Program
    {
        internal static IWebHost BuildWebHost(string[] args)
        {
            var switchMappings = new Dictionary<string, string>
            {
                { "-c", "Configuration" },
            };

            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(new ConfigurationBuilder()
                    .AddCommandLine(args, switchMappings)
                    .Build())
                .UseStartup<Startup>()
                .Build();
        }
    }
}
