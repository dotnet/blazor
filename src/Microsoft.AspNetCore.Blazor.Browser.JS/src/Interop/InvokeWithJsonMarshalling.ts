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
    const asyncJsString = platform.toJavaScriptString(identifier);
    const async = JSON.parse(asyncJsString) as { Success: string, Failure: string, Function: MethodOptions };
    const args = argsJson.map(json => JSON.parse(platform.toJavaScriptString(json)));
    const result = funcInstance.apply(null, args) as Promise<any>;
    result.then(res => {
        invokeDotNetMethod({
            Type: {
                Assembly: async.Function.Type.Assembly,
                TypeName: async.Function.Type.TypeName,
                TypeArguments: {}
            },
            Method: {
                Name: async.Function.Method.Name,
                TypeArguments: {},
                ParameterTypes: async.Function.Method.ParameterTypes
            }
        },
            {
                Argument1: async.Success,
                Argument2: JSON.stringify(res)
            });
    });
}


function jsonReviver(key: string, value: any): any {
  if (value && typeof value === 'object' && value.hasOwnProperty(elementRefKey) && typeof value[elementRefKey] === 'number') {
    return getElementByCaptureId(value[elementRefKey]);
  }

  return value;
}