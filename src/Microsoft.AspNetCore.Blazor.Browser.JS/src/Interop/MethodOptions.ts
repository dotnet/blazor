export interface MethodOptions {
    type: TypeInstance;
    method: MethodInstance;
    Async?: { ResolveId: string, RejectId: string, FunctionName: string }
}

export interface MethodInstance {
    name: string;
    typeArguments: { [key: string]: TypeInstance }
    parameterTypes: TypeInstance[];
}

export interface TypeInstance {
    assembly: string;
    typeName: string;
    TypeArguments: { [key: string]: TypeInstance };
}

export interface DotnetMethodArgumentsList {
    argument1?: any;
    argument2?: any;
}