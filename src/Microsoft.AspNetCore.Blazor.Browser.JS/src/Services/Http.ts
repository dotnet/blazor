import { platform } from '../Environment';
import { MethodHandle, System_String, System_Array, System_Object, Pointer } from '../Platform/Platform';
const httpClientAssembly = 'Microsoft.AspNetCore.Blazor.Browser';
const httpClientNamespace = `${httpClientAssembly}.Http`;
const httpClientTypeName = 'BrowserHttpMessageHandler';
const httpContentTypeName = 'BrowserHttpContent';
const httpReadStreamTypeName = 'BrowserHttpReadStream';
const httpClientFullTypeName = `${httpClientNamespace}.${httpClientTypeName}`;
const pendingResponses = new Map<number, Response>();
const pendingStreams = new Map<number, ReadableStreamReader>();
const pendingChunks = new Map<number, Uint8Array>();
let receiveResponseDataMethod: MethodHandle;
let receiveResponseMethod: MethodHandle;
let streamChunkReadMethod: MethodHandle;
let allocateArrayMethod: MethodHandle;

// These are the functions we're making available for invocation from .NET
export const internalFunctions = {
  supportsStreaming,
  sendAsync,
  getResponseData,
  cleanupFetchRequest,
  readChunk,
  retrieveChunk
}

function supportsStreaming(): boolean {
  return 'body' in Response.prototype && typeof ReadableStream === "function";
}

async function sendAsync(id: number, body: System_Array<any>, jsonFetchArgs: System_String) {
  let response: Response;

  const fetchOptions: FetchOptions = JSON.parse(platform.toJavaScriptString(jsonFetchArgs));
  const requestInit: RequestInit = Object.assign(fetchOptions.requestInit, fetchOptions.requestInitOverrides);

  if (body) {
    requestInit.body = platform.toUint8Array(body);
  }

  try {
    response = await fetch(fetchOptions.requestUri, requestInit);
  } catch (ex) {
    dispatchErrorResponse(id, ex.toString());
    return;
  }

  dispatchSuccessResponse(id, response);
}

function dispatchSuccessResponse(id: number, response: Response) {
  const responseDescriptor: ResponseDescriptor = {
    statusCode: response.status,
    statusText: response.statusText,
    headers: []
  };
  response.headers.forEach((value, name) => {
    responseDescriptor.headers.push([name, value]);
  });

  pendingResponses.set(id, response);

  dispatchResponse(
    id,
    platform.toDotNetString(JSON.stringify(responseDescriptor)),
    /* errorMessage */ null
  );
}

function dispatchErrorResponse(id: number, errorMessage: string) {
  dispatchResponse(
    id,
    /* responseDescriptor */ null,
    platform.toDotNetString(errorMessage)
  );
}

function dispatchResponse(id: number, responseDescriptor: System_String | null, errorMessage: System_String | null) {
  if (!receiveResponseMethod) {
    receiveResponseMethod = platform.findMethod(
      httpClientAssembly,
      httpClientNamespace,
      httpClientTypeName,
      'ReceiveResponse'
    );
  }

  platform.callMethod(receiveResponseMethod, null, [
    platform.toDotNetString(id.toString()),
    responseDescriptor,
    errorMessage,
  ]);
}

async function getResponseData(id: number) {
  let responseData: ArrayBuffer;
  const response = pendingResponses.get(id);
  if (!response) {
    dispatchResponseData(id, null, platform.toDotNetString('Found no response with id ' + id));
    return;
  }

  pendingResponses.delete(id);

  try {
    responseData = await response.arrayBuffer();
  } catch (ex) {
    dispatchResponseData(id, null, ex.toString());
    return;
  }

  if (!allocateArrayMethod) {
    allocateArrayMethod = platform.findMethod(
      httpClientAssembly,
      httpClientNamespace,
      httpContentTypeName,
      'AllocateArray'
    );
  }

  // allocate a managed byte[] of the right size
  const dotNetArray = platform.callMethod(allocateArrayMethod, null, [platform.toDotNetString(responseData.byteLength.toString())]) as System_Array<any>;

  // get an Uint8Array view of it
  const array = platform.toUint8Array(dotNetArray);

  // copy the responseData to our managed byte[]
  array.set(new Uint8Array(responseData));

  dispatchResponseData(id, dotNetArray, null);
}

function dispatchResponseData(id: number, responseData: System_Array<any> | null, errorMessage: System_String | null) {
  if (!receiveResponseDataMethod) {
    receiveResponseDataMethod = platform.findMethod(
      httpClientAssembly,
      httpClientNamespace,
      httpContentTypeName,
      'ReceiveResponseData'
    );
  }

  platform.callMethod(receiveResponseDataMethod, null, [
    platform.toDotNetString(id.toString()),
    responseData,
    errorMessage,
  ]);
}

function cleanupFetchRequest(id: number) {
  pendingResponses.delete(id);
  const reader = pendingStreams.get(id);
  if (reader) {
    pendingStreams.delete(id);
    reader.cancel();
  }
  pendingChunks.delete(id);
}

async function readChunk(id: number) {
  let reader = pendingStreams.get(id);
  if (!reader) {
    const response = pendingResponses.get(id);
    if (!response) {
      dispatchStreamChunkRead(id, null, 'Found no response with id ' + id);
      return;
    }

    if (!response.body || !response.body.getReader) {
      dispatchStreamChunkRead(id, null, 'Streaming responses is not supported');
      return;
    }
    reader = response.body.getReader();
    pendingStreams.set(id, reader);
  }

  try {
    let read = await reader.read();

    // if we're done, signal the end with a zero length read
    if (read.done) {
      cleanupFetchRequest(id);
      dispatchStreamChunkRead(id, 0, null);
      return;
    }

    let value: Uint8Array = read.value;
    pendingChunks.set(id, value);
    dispatchStreamChunkRead(id, value.byteLength, null);
  } catch (ex) {
    if (reader) {
      reader.cancel();
    }
    dispatchStreamChunkRead(id, null, ex.toString());
    return;
  }
}

function retrieveChunk(id: number, arraySpan: System_Object) {
  let chunk = pendingChunks.get(id);
  if (!chunk) {
    throw new Error('Found no response with id ' + id);
  }

  let ptr = platform.getObjectFieldsBaseAddress(arraySpan);
  let buffer = arraySpanReader.buffer(ptr);
  let offset = arraySpanReader.offset(ptr);
  let count = arraySpanReader.count(ptr);
  let array = platform.toUint8Array(buffer);

  // slice our buffer according to offset and count
  if (offset > 0 || count != array.byteLength) {
    array = new Uint8Array(array.buffer, array.byteOffset + offset, count);
  }

  // if we've received more data than we're going to read for now
  if (chunk.byteLength > count) {
    // requeue the leftover
    pendingChunks.set(id, new Uint8Array(chunk.buffer, chunk.byteOffset + count, chunk.byteLength - count));
    // prepare a correctly sized chunk for copying
    chunk = new Uint8Array(chunk.buffer, chunk.byteOffset, count);
  }
  else {
    // we're reading the entire chunk, dequeue it
    pendingChunks.delete(id);
  }

  array.set(chunk);
}

function dispatchStreamChunkRead(id: number, bytesRead: number | null, errorText: string | null) {
  if (!streamChunkReadMethod) {
    streamChunkReadMethod = platform.findMethod(
      httpClientAssembly,
      httpClientNamespace,
      httpReadStreamTypeName,
      'StreamChunkRead'
    );
  }

  platform.callMethod(streamChunkReadMethod, null, [
    platform.toDotNetString(id.toString()),
    bytesRead !== null ? platform.toDotNetString(bytesRead.toString()) : null,
    errorText !== null ? platform.toDotNetString(errorText) : null
  ]);
}

// Keep in sync with memory layout in ArraySpan
const arraySpanReader = {
  buffer: (arraySpan: Pointer) => platform.readObjectField<System_Array<any>>(arraySpan, 0),
  offset: (arraySpan: Pointer) => platform.readInt32Field(arraySpan, 4),
  count: (arraySpan: Pointer) => platform.readInt32Field(arraySpan, 8),
};

// Keep these in sync with the .NET equivalent in BrowserHttpMessageHandler.cs
interface FetchOptions {
  requestUri: string;
  requestInit: RequestInit;
  requestInitOverrides: RequestInit;
}

interface ResponseDescriptor {
  // We don't have BodyText in here because if we did, then in the JSON-response case (which
  // is the most common case), we'd be double-encoding it, since the entire ResponseDescriptor
  // also gets JSON encoded. It would work but is twice the amount of string processing.
  statusCode: number;
  statusText: string;
  headers: string[][];
}
