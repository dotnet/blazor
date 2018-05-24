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
  typeArguments: { [key: string]: TypeInstance }
  parameterTypes: TypeInstance[];
}

export interface TypeInstance {
  assembly: string;
  typeName: string;
  typeArguments: { [key: string]: TypeInstance };
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
