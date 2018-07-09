// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Blazor.Performance;
using Microsoft.AspNetCore.Blazor.Routing;

namespace Microsoft.AspNetCore.BenchmarkDotNet.Runner
{
    internal partial class Program
    {
        static partial void BeforeMain(string[] args)
        {
            if (args.Length == 0 || args[0] != "--profile")
            {
                return;
            }

            Environment.Exit(0);
        }
    }

    public class RouteTableEntryBenchMark
    {
        private RouteEntry[] routeEntryTable;
        private RouteContext contextNoParam;
        private RouteContext contextWithParam;
        private RouteContext contextWithAllParam;

        public RouteTableEntryBenchMark()
        {
            routeEntryTable = new RouteEntry[] {
                    new RouteEntry(TemplateParser.ParseTemplate( "/"), null),
                    new RouteEntry(TemplateParser.ParseTemplate( "/" + string.Join("/", Enumerable.Range(0, 10).Select(i => "{param" + i + "}"))), null)
                };
            contextNoParam = new RouteContext("/");
            contextWithParam = new RouteContext("/" + string.Join("/", Enumerable.Range(0, 5).Select(i => "value")));
            contextWithAllParam = new RouteContext("/" + string.Join("/", Enumerable.Range(0, 10).Select(i => "value")));
        }

        [Benchmark]
        public void Before()
        {
            RouteTable routeTable = new RouteTable(routeEntryTable);
            routeTable.Route(contextNoParam);
        }

        [Benchmark]
        public void After()
        {
            RouteTableEdited routeTable = new RouteTableEdited(routeEntryTable);
            routeTable.Route(contextNoParam);

        }
        [Benchmark]
        public void BeforeWithParam()
        {
            RouteTable routeTable = new RouteTable(routeEntryTable);
            routeTable.Route(contextWithParam);
        }

        [Benchmark]
        public void AfterWithParam()
        {
            RouteTableEdited routeTable = new RouteTableEdited(routeEntryTable);
            routeTable.Route(contextWithParam);

        }
        [Benchmark]
        public void BeforeAllParam()
        {
            RouteTable routeTable = new RouteTable(routeEntryTable);
            routeTable.Route(contextWithAllParam);
        }

        [Benchmark]
        public void AfterAllParam()
        {
            RouteTableEdited routeTable = new RouteTableEdited(routeEntryTable);
            routeTable.Route(contextWithAllParam);

        }

        [Benchmark]
        public void BeforeCtor()
        {
            RouteTable routeTable = new RouteTable(routeEntryTable);
        }

        [Benchmark]
        public void AfterCtor()
        {
            RouteTableEdited routeTable = new RouteTableEdited(routeEntryTable);

        }
    }
}
