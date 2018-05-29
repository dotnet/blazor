import { platform } from '../Environment';
import { System_String } from '../Platform/Platform';
import { getRegisteredFunction } from './RegisteredFunction';
import { error } from 'util';

export interface MethodOptions {
  type: TypeInstance;
  method: MethodInstance;
  async?: AsyncOptions;
}

// Keep in sync with JavaScriptAsync.cs
export interface AsyncOptions {
  callbackId: string;
  functionName: string | MethodOptions;
}

// Keep in sync with DotNetAsync.cs
export interface DotNetAsyncOptions {
  callbackId: string;
  functionName: MethodOptions;
}

// Keep in sync with InvocationResult.cs
export interface InvocationResult {
  succeeded: boolean;
  result?: any;
  message?: string;
}

export interface MethodInstance {
  name: string;
  typeArguments?: { [key: string]: TypeInstance }
  parameterTypes?: TypeInstance[];
}

export interface TypeInstance {
  assembly: string;
  typeName: string;
  typeArguments?: { [key: string]: TypeInstance };
}

export interface DotnetMethodArgumentsList {
  argument1?: any;
  argument2?: any;
  argument3?: any;
  argument4?: any;
  argument5?: any;
  argument6?: any;
  argument7?: any;
  argument8?: any;
}


export function invokeDotNetMethod<T>(methodOptions: MethodOptions, ...args: any[]): (T | null) {
  const method = platform.findMethod(
    "Microsoft.AspNetCore.Blazor.Browser",
    "Microsoft.AspNetCore.Blazor.Browser.Interop",
    "JavaScriptInvoke",
    "InvokeDotnetMethod");

  const packedArgs = packArguments(args);

  const serializedOptions = platform.toDotNetString(JSON.stringify(methodOptions));
  const serializedArgs = platform.toDotNetString(JSON.stringify(packedArgs));
  const serializedResult = platform.callMethod(method, null, [serializedOptions, serializedArgs]);

  if (serializedResult !== null && serializedResult !== undefined && (serializedResult as any) !== 0) {
    const result = JSON.parse(platform.toJavaScriptString(serializedResult as System_String));
    if (result.succeeded) {
      return result.result;
    } else {
      throw new Error(result.message);
    }
  }

  return null;
}

let globalId = 0;

export function invokeDotNetMethodAsync<T>(methodOptions: MethodOptions, ...args: any[]): Promise<T | null> {
  const callbackId = (globalId++).toString();
  methodOptions.async = { callbackId: callbackId, functionName: "invokeJavaScriptCallback" };

  const result = new Promise<T | null>((resolve, reject) => {
    TrackedReference.track(callbackId, (invocationResult: InvocationResult) => {
      // We got invoked, so we unregister ourselves.
      TrackedReference.untrack(callbackId);
      if (invocationResult.succeeded) {
        resolve(invocationResult.result);
      } else {
        reject(new Error(invocationResult.message));
      }
    });
  });

  invokeDotNetMethod(methodOptions, ...args);

  return result;
}

export function invokeJavaScriptCallback(id: string, invocationResult: InvocationResult): void {
  const callbackRef = TrackedReference.get(id);
  const callback = callbackRef.trackedObject as Function;
  callback.call(null, invocationResult);
}

function packArguments(args: any[]): DotnetMethodArgumentsList {
  var result = {};
  if (args.length == 0) {
    return result;
  }

  if (args.length > 7) {
    for (let i = 0; i < 7; i++) {
      result[`argument${[i + 1]}`] = args[i];
    }
    result['argument8'] = packArguments(args.slice(7));
  } else {
    for (let i = 0; i < args.length; i++) {
      result[`argument${[i + 1]}`] = args[i];
    }
  }

  return result;
}

class TrackedReference {
  private static references: Map<string, any> = new Map<string, any>();

  private constructor(public id: string, public trackedObject: any) {
  }

  public static track(id: string, trackedObject: any): void {
    const ref = new TrackedReference(id, trackedObject);
    const refs = TrackedReference.references;
    if (refs.has(id)) {
      throw new Error(`An element with id '${id}' is already being tracked.`);
    }

    refs.set(id, ref);
  }

  public static untrack(id: string): void {
    const refs = TrackedReference.references;
    if (!refs.has(id)) {
      throw new Error(`An element with id '${id}' is not being being tracked.`);
    }

    refs.delete(id);
  }

  public static get(id: string): TrackedReference {
    const refs = TrackedReference.references;
    if (!refs.has(id)) {
      throw new Error(`An element with id '${id}' is not being being tracked.`);
    }

    return refs.get(id);
  }
}
