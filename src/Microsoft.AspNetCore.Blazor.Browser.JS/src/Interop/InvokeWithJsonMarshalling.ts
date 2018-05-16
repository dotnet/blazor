import { platform } from '../Environment';
import { System_String } from '../Platform/Platform';
import { getRegisteredFunction } from './RegisteredFunction';
import { invokeDotNetMethod } from './DotNetInvoker';
import { getElementByCaptureId } from '../Rendering/ElementReferenceCapture';
import { MethodOptions } from './MethodOptions'

const elementRefKey = '_blazorElementRef'; // Keep in sync with ElementRef.cs

export function invokeWithJsonMarshalling(identifier: System_String, ...argsJson: System_String[]) {
  const identifierJsString = platform.toJavaScriptString(identifier);
  const funcInstance = getRegisteredFunction(identifierJsString);
  const args = argsJson.map(json => JSON.parse(platform.toJavaScriptString(json), jsonReviver));
  const result = funcInstance.apply(null, args);
  if (result !== null && result !== undefined) {
    const resultJson = JSON.stringify(result);
    return platform.toDotNetString(resultJson);
  } else {
    return null;
  }
}

export function invokeWithJsonMarshallingAsync(identifier: System_String, asyncProtocol: System_String, ...argsJson: System_String[]) {
    const identifierJsString = platform.toJavaScriptString(identifier);
    const funcInstance = getRegisteredFunction(identifierJsString);
    const asyncJsString = platform.toJavaScriptString(asyncProtocol);
    const async = JSON.parse(asyncJsString) as { success: string, failure: string, function: MethodOptions };
    const args = argsJson.map(json => JSON.parse(platform.toJavaScriptString(json)));
    const result = funcInstance.apply(null, args) as Promise<any>;
    result.then(res => {
        invokeDotNetMethod({
            type: {
                assembly: async.function.type.assembly,
                typeName: async.function.type.typeName,
                TypeArguments: {}
            },
            method: {
                name: async.function.method.name,
                typeArguments: {},
                parameterTypes: async.function.method.parameterTypes
            }
        },
            {
                argument1: async.success,
                argument2: JSON.stringify(res)
            });
    });
}


function jsonReviver(key: string, value: any): any {
  if (value && typeof value === 'object' && value.hasOwnProperty(elementRefKey) && typeof value[elementRefKey] === 'number') {
    return getElementByCaptureId(value[elementRefKey]);
  }

  return value;
}