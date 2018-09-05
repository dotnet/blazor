// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.JSInterop;
using Mono.WebAssembly.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Blazor.Browser.Http
{
    class BrowserHttpReadStream : Stream
    {
        static readonly IDictionary<int, TaskCompletionSource<int>> _pendingResponses
            = new Dictionary<int, TaskCompletionSource<int>>();

        readonly int _requestId;
        int _bufferedBytesRemaining;

        // temporary storage for passing to js
        ArraySpan _arraySpan = new ArraySpan();

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public BrowserHttpReadStream(int requestId)
        {
            _requestId = requestId;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if (count < 0 || buffer.Length - offset < count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (_bufferedBytesRemaining > 0)
            {
                return ReadBuffered();
            }

            var tcs = new TaskCompletionSource<int>();
            _pendingResponses.Add(_requestId, tcs);

            ((MonoWebAssemblyJSRuntime)JSRuntime.Current).InvokeUnmarshalled<int, object>(
                "Blazor._internal.http.readChunk",
                _requestId);

            _bufferedBytesRemaining = await tcs.Task;
            if (_bufferedBytesRemaining == 0)
            {
                return 0;
            }

            return ReadBuffered();

            int ReadBuffered()
            {
                _arraySpan.Buffer = buffer;
                _arraySpan.Offset = offset;
                _arraySpan.Count = count;

                ((MonoWebAssemblyJSRuntime)JSRuntime.Current).InvokeUnmarshalled<int, ArraySpan, object>(
                    "Blazor._internal.http.retrieveChunk",
                    _requestId,
                    _arraySpan);

                _arraySpan.Buffer = default;
                _arraySpan.Offset = default;
                _arraySpan.Count = default;

                int bytesRead = Math.Min(_bufferedBytesRemaining, count);
                _bufferedBytesRemaining = Math.Max(0, _bufferedBytesRemaining - count);
                return bytesRead;
            }
        }

        private static void StreamChunkRead(string id, string bytesRead, string errorText)
        {
            var idVal = Int32.Parse(id);

            var tcs = _pendingResponses[idVal];
            _pendingResponses.Remove(idVal);

            if (errorText != null)
            {
                tcs.SetException(new HttpRequestException(errorText));
            }
            else
            {
                tcs.SetResult(Int32.Parse(bytesRead));
            }
        }

        protected override void Dispose(bool disposing)
        {
            ((MonoWebAssemblyJSRuntime)JSRuntime.Current).InvokeUnmarshalled<int, object>(
                "Blazor._internal.http.cleanupFetchRequest",
                _requestId);
        }

        ~BrowserHttpReadStream()
        {
            Dispose(false);
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new PlatformNotSupportedException("Synchronous reads are not supported, use ReadAsync instead");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        class ArraySpan
        {
            public byte[] Buffer { get; set; }
            public int Offset { get; set; }
            public int Count { get; set; }
        }
    }
}
