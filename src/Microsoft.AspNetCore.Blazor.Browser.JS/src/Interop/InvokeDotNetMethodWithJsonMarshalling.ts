import { platform } from '../Environment';
import { System_String, Pointer } from '../Platform/Platform';
import { getRegisteredFunction } from './RegisteredFunction';
import { error } from 'util';

export interface MethodOptions {
  type: TypeInstance;
  method: MethodInstance;
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
  name: string;
  typeArguments?: { [key: string]: TypeInstance };
}

export function invokeDotNetMethod<T>(methodOptions: MethodOptions, ...args: any[]): (T | null) {
  return invokeDotNetMethodCore(methodOptions, null, ...args);
}

let registrations = {};

function resolveRegistration(methodOptions: MethodOptions) {
  let existing = registrations[methodOptions.type.assembly];
  existing = existing && registrations[methodOptions.type.name];
  existing = existing && registrations[methodOptions.method.name];
  if (existing !== undefined) {
    return existing;
  } else {
    const method = platform.findMethod(
      "Microsoft.AspNetCore.Blazor.Browser",
      "Microsoft.AspNetCore.Blazor.Browser.Interop",
      "JavaScriptInvoke",
      "FindDotNetMethod");

    const serializedOptions = platform.toDotNetString(JSON.stringify(methodOptions));
    const result = platform.callMethod(method, null, [serializedOptions]);
    const registration = platform.toJavaScriptString(result as System_String);

    if (registrations[methodOptions.type.assembly] === undefined) {
      let assembly = {};
      let type = {};
      registrations[methodOptions.type.assembly] = assembly;
      assembly[methodOptions.type.name] = type;
      type[methodOptions.method.name] = registration;
    } else if (registrations[methodOptions.type.assembly][methodOptions.type.assembly] === undefined) {
      let type = {};
      registrations[methodOptions.type.assembly][methodOptions.type.name] = type;
      type[methodOptions.type.name] = type;
      type[methodOptions.method.name] = registration;
    } else {
      registrations[methodOptions.type.assembly][methodOptions.type.name][methodOptions.method.name] = registration;
    }

    return registration;
  }
}

function invokeDotNetMethodCore<T>(methodOptions: MethodOptions, callbackId: string | null, ...args: any[]): (T | null) {
  const method = platform.findMethod(
    "Microsoft.AspNetCore.Blazor.Browser",
    "Microsoft.AspNetCore.Blazor.Browser.Interop",
    "JavaScriptInvoke",
    "InvokeDotNetMethod");

  var registration = resolveRegistration(methodOptions);

  const packedArgs = packArguments(args);

  const serializedCallback = callbackId != null ? platform.toDotNetString(callbackId) : null;
  const serializedArgs = platform.toDotNetString(JSON.stringify(packedArgs));
  const serializedRegistration = platform.toDotNetString(registration);
  const serializedResult = platform.callMethod(method, null, [serializedRegistration, serializedCallback, serializedArgs]);

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

// We don't have to worry about overflows here. Number.MAX_SAFE_INTEGER in JS is 2^53-1
let globalId = 0;

export function invokeDotNetMethodAsync<T>(methodOptions: MethodOptions, ...args: any[]): Promise<T | null> {
  const callbackId = (globalId++).toString();

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

  invokeDotNetMethodCore(methodOptions, callbackId, ...args);

  return result;
}

export function invokePromiseCallback(id: string, invocationResult: InvocationResult): void {
  const callbackRef = TrackedReference.get(id);
  const callback = callbackRef.trackedObject as Function;
  callback.call(null, invocationResult);
}

function packArguments(args: any[]) {
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
