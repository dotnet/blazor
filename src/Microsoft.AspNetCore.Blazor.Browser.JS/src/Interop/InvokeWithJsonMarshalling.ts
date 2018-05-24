import { platform } from '../Environment';
import { System_String } from '../Platform/Platform';
import { getRegisteredFunction } from './RegisteredFunction';
import { invokeDotNetMethod } from './DotNetInvoker';
import { getElementByCaptureId } from '../Rendering/ElementReferenceCapture';
import { MethodOptions, AsyncOptions, DotNetAsyncOptions, InvocationResult } from './MethodOptions'
import { System } from 'typescript';
import { error } from 'util';

const elementRefKey = '_blazorElementRef'; // Keep in sync with ElementRef.cs

export function invokeWithJsonMarshalling(identifier: System_String, ...argsJson: System_String[]) {
  let result: InvocationResult;
  const identifierJsString = platform.toJavaScriptString(identifier);
  const args = argsJson.map(json => JSON.parse(platform.toJavaScriptString(json), jsonReviver));

  try {
    result = { succeeded: true, result: invokeWithJsonMarshallingCore(identifierJsString, ...args) };
  } catch (e) {
    result = { succeeded: false, message: e.message };
  }

  const resultJson = JSON.stringify(result);
  return platform.toDotNetString(resultJson);
}

function invokeWithJsonMarshallingCore(identifier: string, ...args: any[]) {
  const funcInstance = getRegisteredFunction(identifier);
  const result = funcInstance.apply(null, args);
  if (result !== null && result !== undefined) {
    return result;
  } else {
    return null;
  }
}

export function invokeWithJsonMarshallingAsync<T>(identifier: string, asyncProtocol: string, ...argsJson: string[]) {
  const async = JSON.parse(asyncProtocol) as DotNetAsyncOptions;
  let invocationResult: InvocationResult;
    const result = invokeWithJsonMarshallingCore(identifier, ...argsJson) as Promise<any>;

    result
      .then(res => invokeDotNetMethod(async.functionName, async.callbackId, JSON.stringify({ succeeded: true, result: res })))
      .catch(reason => invokeDotNetMethod(async.functionName, async.callbackId, JSON.stringify({ succeeded: false, message: (reason && reason.message) || (reason && reason.toString && reason.toString()) })));
    invocationResult = { succeeded: true, result: null };

  return null;
}


function jsonReviver(key: string, value: any): any {
  if (value && typeof value === 'object' && value.hasOwnProperty(elementRefKey) && typeof value[elementRefKey] === 'number') {
    return getElementByCaptureId(value[elementRefKey]);
  }

  return value;
}
