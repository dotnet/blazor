import { platform } from '../Environment';
import { MethodHandle, System_String, System_Array } from '../Platform/Platform';
const httpClientAssembly = 'Microsoft.AspNetCore.Blazor.Browser';
const httpClientNamespace = `${httpClientAssembly}.Http`;
const httpClientTypeName = 'BrowserHttpMessageHandler';
const httpContentTypeName = 'BrowserHttpContent';
const httpClientFullTypeName = `${httpClientNamespace}.${httpClientTypeName}`;
const pendingResponses = new Map<number, Response>();
let receiveResponseDataMethod: MethodHandle;
let receiveResponseMethod: MethodHandle;
let allocateArrayMethod: MethodHandle;

// These are the functions we're making available for invocation from .NET
export const internalFunctions = {
  sendAsync,
  getResponseData,
  discardResponse
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

function discardResponse(id: number) {
  pendingResponses.delete(id);
}

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
