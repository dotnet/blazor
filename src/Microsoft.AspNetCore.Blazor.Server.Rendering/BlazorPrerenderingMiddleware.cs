using System;
using System.IO;
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
        private readonly RequestDelegate _next;

        public BlazorPrerenderingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {


            return _next(context);
        }

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

                    using (var stream = fileInfo.CreateReadStream())
                    using (var reader = new StreamReader(stream))
                    {
                        var html = reader.ReadToEnd();

                        var tokenizer = new HtmlTokenizer(new TextSource(html), HtmlEntityService.Resolver);


                    }

                    context.Response.StatusCode = 200;
                    context.Response.ContentType = contentType;
                    context.Response.ContentLength = fileInfo.Length;
                    //stream.CopyTo(context.Response.Body);

                    return Task.FromResult(0);
                }

                return next();
            });
        }
    }

}