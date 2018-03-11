using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html;
using AngleSharp.Parser.Html;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.AspNetCore.SpaServices.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Blazor.Server.Rendering
{
    public class BlazorPrerenderingMiddleware
    {
        public static void Attach(ISpaBuilder spaBuilder)
        {
            if (spaBuilder == null)
                throw new ArgumentNullException(nameof(spaBuilder));
            IApplicationBuilder applicationBuilder = spaBuilder.ApplicationBuilder;

            SpaOptions options = spaBuilder.Options;
            var fileOptions = options.DefaultPageStaticFileOptions;
            applicationBuilder.Use((context, next) =>
            {
                context.Request.Path = options.DefaultPage;
                return next();
            });

            applicationBuilder.Use((context, next) =>
            {
                var subpath = context.Request.Path;
                var fileInfo = fileOptions.FileProvider.GetFileInfo(subpath);
                if (fileInfo.Exists)
                {
                    fileOptions.ContentTypeProvider.TryGetContentType(subpath, out var contentType);

                    context.Response.StatusCode = 200;
                    context.Response.ContentType = contentType;

                    using (var stream = fileInfo.CreateReadStream())
                    using (var reader = new StreamReader(stream))
                    {
                        var html = reader.ReadToEnd();

                        var content = Prerender(html);
                        context.Response.ContentLength = content.Length;
                        using (var writer = new StreamWriter(context.Response.Body, Encoding.Default, 4096, leaveOpen: true))
                        {
                            writer.Write(content);
                        }
                    }

                    return Task.FromResult(0);
                }

                return next();
            });
        }

        private static string Prerender(string html)
        {
            var tokenizer = new HtmlTokenizer(new TextSource(html), HtmlEntityService.Resolver);

            while (true)
            {
                var token = tokenizer.Get();
                switch (token.Type)
                {
                    case HtmlTokenType.EndOfFile:
                        return @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <title>Sample Blazor app</title>
</head>
<body>
    <h1>Hello prerendering</h1>
</body>
</html>";
                }
            }
        }
    }

}