// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.JSInterop;
using Mono.WebAssembly.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Blazor.Browser.Http
{
    class BrowserHttpContent : HttpContent
    {
        static readonly IDictionary<int, TaskCompletionSource<byte[]>> _pendingResponses
            = new Dictionary<int, TaskCompletionSource<byte[]>>();

        readonly int _requestId;
        byte[] _data;

        public BrowserHttpContent(int requestId)
        {
            _requestId = requestId;
        }

        private async Task<byte[]> GetResponseData()
        {
            if (_data != null)
            {
                return _data;
            }

            var tcs = new TaskCompletionSource<byte[]>();
            _pendingResponses.Add(_requestId, tcs);

            ((MonoWebAssemblyJSRuntime)JSRuntime.Current).InvokeUnmarshalled<int, object>(
                "Blazor._internal.http.getResponseData",
                _requestId);

            _data = await tcs.Task;
            return _data;
        }

        private static byte[] AllocateArray(string length)
        {
            return new byte[Int32.Parse(length)];
        }

        private static void ReceiveResponseData(
            string id,
            byte[] responseData,
            string errorText)
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
                tcs.SetResult(responseData);
            }
        }

        private void CleanupFetchRequest()
        {
            if (_data == null)
            {
                ((MonoWebAssemblyJSRuntime)JSRuntime.Current).InvokeUnmarshalled<int, object>(
                    "Blazor._internal.http.cleanupFetchRequest",
                    _requestId);
            }
        }

        protected override async Task<Stream> CreateContentReadStreamAsync()
        {
            var data = await GetResponseData();
            return new MemoryStream(data, writable: false);
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var data = await GetResponseData();
            await stream.WriteAsync(data, 0, data.Length);
        }

        protected override bool TryComputeLength(out long length)
        {
            if (_data != null)
            {
                length = _data.Length;
                return true;
            }

            length = 0;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            CleanupFetchRequest();
            base.Dispose(disposing);
        }

        ~BrowserHttpContent()
        {
            Dispose(false);
        }
    }
}
