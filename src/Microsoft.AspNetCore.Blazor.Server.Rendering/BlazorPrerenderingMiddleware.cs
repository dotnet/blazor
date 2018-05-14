using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html;
using AngleSharp.Parser.Html;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Blazor.Server.Rendering
{
    /// <summary>
    /// 
    /// </summary>
    public class BlazorPrerenderingMiddleware
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="spaBuilder"></param>
        /// <param name="entryTagName"></param>
        /// <param name="configure"></param>
        public static void Attach<T>(ISpaBuilder spaBuilder, string entryTagName, Action<IServiceCollection> configure)
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

            var col = new PreServiceProvider(configure);
            applicationBuilder.Use((context, next) =>
            {
                var renderer = new PreRenderer(col);
                var content = renderer.Render<T>();

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

                        var responseContent = Prerender(html, entryTagName, content);
                        context.Response.ContentLength = responseContent.Length;
                        using (var writer = new StreamWriter(context.Response.Body))
                        {
                            writer.Write(responseContent);
                        }
                    }

                    return Task.FromResult(0);
                }

                return next();
            });
        }

        private static string Prerender(string htmlTemplate, string entryTagName, string content)
        {
            // copied from IndexHtmlFileProvider.cs
            var resultBuilder = new StringBuilder();

            // Search for a tag of the form <app></app>, and replace
            // it with the prerendered content
            var tokenizer = new HtmlTokenizer(
                new TextSource(htmlTemplate),
                HtmlEntityService.Resolver);
            var currentRangeStartPos = 0;
            var isInBlazorBootTag = false;
            var resumeOnNextToken = false;
            while (true)
            {
                var token = tokenizer.Get();
                if (resumeOnNextToken)
                {
                    resumeOnNextToken = false;
                    currentRangeStartPos = token.Position.Position;
                }

                switch (token.Type)
                {
                    case HtmlTokenType.StartTag:
                        {
                            // Only do anything special if this is a Blazor boot tag
                            var tag = token.AsTag();
                            if (IsEntryTag(tag, entryTagName))
                            {
                                // First, emit the original source text prior to this special tag, since
                                // we want that to be unchanged
                                resultBuilder.Append(htmlTemplate, currentRangeStartPos, token.Position.Position - currentRangeStartPos - 1);

                                // Instead of emitting the source text for this special tag, emit a fully-
                                // configured Blazor boot script tag
                                AppendContent(resultBuilder, entryTagName, content);

                                // Set a flag so we know not to emit anything else until the special
                                // tag is closed
                                isInBlazorBootTag = true;
                            }
                            break;
                        }

                    case HtmlTokenType.EndTag:
                        // If this is an end tag corresponding to the Blazor boot script tag, we
                        // can switch back into the mode of emitting the original source text
                        if (isInBlazorBootTag)
                        {
                            isInBlazorBootTag = false;
                            resumeOnNextToken = true;
                        }
                        break;

                    case HtmlTokenType.EndOfFile:
                        // Finally, emit any remaining text from the original source file
                        resultBuilder.Append(htmlTemplate, currentRangeStartPos, htmlTemplate.Length - currentRangeStartPos);
                        return resultBuilder.ToString();
                }
            }
        }

        private static bool IsEntryTag(HtmlTagToken tag, string entryTagName)
            => string.Equals(tag.Name, entryTagName, StringComparison.Ordinal);

        private static void AppendContent(
            StringBuilder resultBuilder, string tagName, string content)
        {
            resultBuilder.AppendLine($@"<{tagName}>
{content}
</{tagName}>");
        }
    }

}