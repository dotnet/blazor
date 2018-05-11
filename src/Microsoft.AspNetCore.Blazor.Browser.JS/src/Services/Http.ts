﻿import { registerFunction } from '../Interop/RegisteredFunction';
import { platform } from '../Environment';
import { MethodHandle, System_String, System_Array } from '../Platform/Platform';
const httpClientAssembly = 'Microsoft.AspNetCore.Blazor.Browser';
const httpClientNamespace = `${httpClientAssembly}.Http`;
const httpClientTypeName = 'BrowserHttpMessageHandler';
const httpClientFullTypeName = `${httpClientNamespace}.${httpClientTypeName}`;
let receiveResponseMethod: MethodHandle;

registerFunction(`${httpClientFullTypeName}.Send`, (id: number, body: System_Array<any>, jsonFetchArgs: System_String) => {
  sendAsync(id, body, jsonFetchArgs);
});

async function sendAsync(id: number, body: System_Array<any>, jsonFetchArgs: System_String) {
  let response: Response;
  let responseText: string;

  const fetchOptions: FetchOptions = JSON.parse(platform.toJavaScriptString(jsonFetchArgs));
  const requestInit: RequestInit = Object.assign(fetchOptions.requestInit, fetchOptions.requestInitOverrides);

  if (body) {
    requestInit.body = platform.toUint8Array(body);
  }

  try {
    response = await fetch(fetchOptions.requestUri, requestInit);
    responseData = await response.arrayBuffer();
  } catch (ex) {
    dispatchErrorResponse(id, ex.toString());
    return;
  }

  dispatchSuccessResponse(id, response, responseText);
}

function dispatchSuccessResponse(id: number, response: Response, responseText: string) {
  const responseDescriptor: ResponseDescriptor = {
    statusCode: response.status,
    statusText: response.statusText,
    headers: []
  };
  response.headers.forEach((value, name) => {
    responseDescriptor.headers.push([name, value]);
  });

  dispatchResponse(
    id,
    platform.toDotNetString(JSON.stringify(responseDescriptor)),
    platform.toDotNetString(responseText), // TODO: Consider how to handle non-string responses
    /* errorMessage */ null
  );
}

function dispatchErrorResponse(id: number, errorMessage: string) {
  dispatchResponse(
    id,
    /* responseDescriptor */ null,
    /* responseText */ null,
    platform.toDotNetString(errorMessage)
  );
}

function dispatchResponse(id: number, responseDescriptor: System_String | null, responseText: System_String | null, errorMessage: System_String | null) {
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
    responseText,
    errorMessage,
  ]);
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
