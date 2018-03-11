// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Blazor.Browser.Http;
using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.Rendering;
using Microsoft.AspNetCore.Blazor.RenderTree;
using Microsoft.AspNetCore.Blazor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace PrerenderingApp.Client
{
    public class Program
    {
        static void Main(string[] args)
        {
            new BrowserRenderer().AddComponent<Home>("app");
        }
    }
}
