// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Net.Mime;

namespace Microsoft.AspNetCore.Blazor.Cli.Server
{
    internal class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    MediaTypeNames.Application.Octet,
                    WasmMediaTypeNames.Application.Wasm
                });
            });
        }

        public void Configure(IApplicationBuilder app, IConfiguration configuration)
        {
            app.UseDeveloperExceptionPage();
            app.UseResponseCompression();
            EnableConfiguredPathbase(app, configuration);

            var clientAssemblyPath = FindClientAssemblyPath(app, configuration);
            app.UseBlazor(new BlazorOptions { ClientAssemblyPath = clientAssemblyPath });
        }

        private static void EnableConfiguredPathbase(IApplicationBuilder app, IConfiguration configuration)
        {
            var pathBase = configuration.GetValue<string>("pathbase");
            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(pathBase);

                // To ensure consistency with a production environment, only handle requests
                // that match the specified pathbase.
                app.Use((context, next) =>
                {
                    if (context.Request.PathBase == pathBase)
                    {
                        return next();
                    }
                    else
                    {
                        context.Response.StatusCode = 404;
                        return context.Response.WriteAsync($"The server is configured only to " +
                            $"handle request URIs within the PathBase '{pathBase}'.");
                    }
                });
            }
        }

        private static string FindClientAssemblyPath(IApplicationBuilder app, IConfiguration configuration)
        {
            var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            var contentRoot = env.ContentRootPath;
            var buildConfiguration = configuration["Configuration"] ?? "Debug";
            var binDir = FindClientBinDir(contentRoot, buildConfiguration);
            var appName = Path.GetFileName(contentRoot); // TODO: Allow for the possibility that the assembly name has been overridden
            var assemblyPath = Path.Combine(binDir, $"{appName}.dll");
            if (!File.Exists(assemblyPath))
            {
                throw new FileNotFoundException($"Could not locate application assembly at expected location {assemblyPath}");
            }

            return assemblyPath;
        }

        private static string FindClientBinDir(string clientAppSourceRoot, string buildConfiguration)
        {
            var binDir = Path.Combine(clientAppSourceRoot, "bin", buildConfiguration);
            var binDirExists = Directory.Exists(binDir);
            if (!binDirExists)
            {
                throw new InvalidOperationException($"Cannot find bin directory for Blazor app. " +
                    $"Folder '{binDir}' does not exists on disk. Make sure the project has been built.");
            }

            var subdirectories = Directory.GetDirectories(binDir);
            if (subdirectories.Length != 1)
            {
                throw new InvalidOperationException($"Could not locate bin directory for Blazor app. " +
                    $"Expected to find exactly 1 subdirectory in '{binDir}', but found {subdirectories.Length}.");
            }

            return Path.Combine(binDir, subdirectories[0]);
        }
    }
}
