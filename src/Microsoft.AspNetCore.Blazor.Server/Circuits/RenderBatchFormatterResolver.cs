// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using MessagePack;
using MessagePack.Formatters;
using Microsoft.AspNetCore.Blazor.Rendering;
using System;
using System.IO;
using System.IO.Compression;

namespace Microsoft.AspNetCore.Blazor.Server.Circuits
{
    /// <summary>
    /// A MessagePack IFormatterResolver that provides an efficient binary serialization
    /// of <see cref="RenderBatch"/>. The client-side code knows how to walk through this
    /// binary representation directly, without it first being parsed as an object graph.
    /// </summary>
    internal class RenderBatchFormatterResolver : IFormatterResolver
    {
        public IMessagePackFormatter<T> GetFormatter<T>()
            => typeof(T) == typeof(RenderBatch) ? (IMessagePackFormatter<T>)RenderBatchFormatter.Instance : null;

        private class RenderBatchFormatter : IMessagePackFormatter<RenderBatch>
        {
            public static readonly RenderBatchFormatter Instance = new RenderBatchFormatter();

            // No need to accept incoming RenderBatch
            public RenderBatch Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
                => throw new NotImplementedException();

            public int Serialize(ref byte[] bytes, int offset, RenderBatch value, IFormatterResolver formatterResolver)
            {
                // TODO: Instead of directly using MemoryStream here and in CompressStream below,
                // consider using https://github.com/Microsoft/Microsoft.IO.RecyclableMemoryStream
                // That automatically provides pooling for the underlying buffers and avoids using
                // the large object heap where possible. The logic here is already written to
                // avoid calling memoryStream.GetBuffer() or memoryStream.ToArray(), so it should
                // be possible to use RecyclableMemoryStream as a drop-in replacement.

                using (var memoryStream = new MemoryStream())
                using (var renderBatchWriter = new RenderBatchWriter(memoryStream, leaveOpen: false))
                {
                    renderBatchWriter.Write(value);

                    var compressedStream = CompressStream(memoryStream);
                    try
                    {
                        return CopyStreamToMessagePack(compressedStream, ref bytes, offset);
                    }
                    finally
                    {
                        compressedStream.Dispose();
                    }
                }
            }

            private static Stream CompressStream(Stream input)
            {
                var output = new MemoryStream();
                using (var gzipStream = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
                {
                    input.Seek(0, SeekOrigin.Begin);
                    input.CopyTo(gzipStream);
                    gzipStream.Flush();
                    return output;
                }
            }

            private static int CopyStreamToMessagePack(Stream stream, ref byte[] bytes, int offset)
            {
                var buffer = new byte[32768];
                int read;

                stream.Seek(0, SeekOrigin.Begin);
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    offset += MessagePackBinary.WriteBytes(ref bytes, offset, buffer, 0, read);
                }

                return offset;
            }
        }
    }
}
