// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Blazor.Build.Core
{
    internal static class AppBuilder
    {
        internal static void Execute(string assemblyPath, string indexHtml, string[] references, string outputPath)
        {
            IndexHtmlWriter.UpdateIndex(indexHtml, assemblyPath, references, outputPath);
        }
    }
}
