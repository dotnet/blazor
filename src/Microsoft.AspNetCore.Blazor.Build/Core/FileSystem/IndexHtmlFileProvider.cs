// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.Internal.Common.FileProviders;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.FileProviders;
using System.Linq;
using AngleSharp.Parser.Html;
using AngleSharp.Dom;

namespace Microsoft.AspNetCore.Blazor.Build.Core.FileSystem
{
    internal class IndexHtmlFileProvider : InMemoryFileProvider
    {
        public IndexHtmlFileProvider(string htmlTemplate, string assemblyName, IEnumerable<IFileInfo> binFiles)
            : base(ComputeContents(htmlTemplate, assemblyName, binFiles))
        {
        }

        private static IEnumerable<(string, byte[])> ComputeContents(string htmlTemplate, string assemblyName, IEnumerable<IFileInfo> binFiles)
        {
            if (htmlTemplate != null)
            {
                var html = GetIndexHtmlContents(htmlTemplate, assemblyName, binFiles);
                var htmlBytes = Encoding.UTF8.GetBytes(html);
                yield return ("/index.html", htmlBytes);
            }
        }

        /// <summary>
        /// Injects either the Blazor boot code or just the configuration data supporting
        /// boot.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the template already contains a &lt;script&gt; tag that has a
        /// <c>blazor</c> attribute with a value of <c>boot</c>, then
        /// it is expected that the tag is responsible for loading the Blazor
        /// boot script.  In this case a script element containing only boot
        /// configuration is inserted at the very top of the body.
        /// </para><para>
        /// Otherwise, we inject a new script tag that includes both the configuration
        /// data and the Blazor boot code and we inject just before the first user-owned
        /// &lt;script&gt; tag we find in body, or at the very end of the body if no other
        /// script tag is found.  This prevents blocking the page rendering while fetching
        /// the script.
        /// </para>
        /// </remarks>
        private static string GetIndexHtmlContents(string htmlTemplate, string assemblyName, IEnumerable<IFileInfo> binFiles)
        {
            // TODO: Instead of inserting the script as the first element in <body>,
            // consider either:
            // [1] Inserting it just before the first <script> in the <body>, so that
            //     developers can still put other <script> elems after (and therefore
            //     reference Blazor JS APIs from them) but we don't block the page
            //     rendering while fetching that script. Note that adding async/defer
            //     alone isn't enough because that doesn't help older browsers that
            //     don't suppor them.
            // [2] Or possibly better, don't insert the <script> magically at all.
            //     Instead, just insert a block of configuration data at the top of
            //     <body> (e.g., <script type='blazor-config'>{ ...json... }</script>)
            //     and then let the developer manually place the tag that loads blazor.js
            //     wherever they want (adding their own async/defer if they want).

            // ORIGINAL CODE:
            //return htmlTemplate
            //    .Replace("<body>", "<body>\n" + CreateBootMarkup(assemblyName, binFiles));

            var parser = new HtmlParser();
            var dom = parser.Parse(htmlTemplate);

            // First see if the user has declared a 'boot' script,
            // then it's their responsibility to load blazor.js
            var bootScript = dom.Body?.QuerySelectorAll("script")
                   .Where(x => x.Attributes["blazor"]?.Value == "boot").FirstOrDefault();
            if (bootScript != null)
            {
                // We just insert only blazor config data at the top
                var newScript = new DocumentFragment((Element)dom.Body,
                    CreateBootMarkup(assemblyName, binFiles, loadJs: false));
                ((Element)dom.Body).InsertChild(0, newScript);
            }
            else
            {
                // We insert the loader script
                var newScript = new DocumentFragment((Element)dom.Body,
                    CreateBootMarkup(assemblyName, binFiles));

                // If no boot script, get the first script if there is one
                var firstScript = dom.Body?.QuerySelector("script");
                if (firstScript != null)
                {
                    // So we can inject our own boot script just before it
                    dom.Body.InsertBefore(newScript, firstScript);
                }
                else if (dom.Body != null)
                {
                    // No user-provided script, guess we can load at the very end
                    dom.Body.AppendChild(newScript);
                }
            }

            return dom.DocumentElement.OuterHtml;
        }

        private static string CreateBootMarkup(string assemblyName, IEnumerable<IFileInfo> binFiles,
            bool loadJs = true)
        {
            var assemblyNameWithExtension = $"{assemblyName}.dll";
            var referenceNames = binFiles
                .Where(file => !string.Equals(file.Name, assemblyNameWithExtension))
                .Select(file => file.Name);
            var referencesAttribute = string.Join(",", referenceNames.ToArray());

            var scriptSrcOrType = "type=\"blazor-config\"";
            var scriptType = "config";
            if (loadJs)
            {
                scriptSrcOrType = $"src=\"/_framework/blazor.js\"";
                scriptType = "boot";
            }

            return $"<script {scriptSrcOrType}" +
                   $" blazor=\"{scriptType}\"" +
                   $" main=\"{assemblyNameWithExtension}\"" +
                   $" references=\"{referencesAttribute}\"></script>";
        }
    }
}
