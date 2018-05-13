export interface MethodOptions {
    Type: TypeInstance;
    Method: MethodInstance;
    Async?: { ResolveId: string, RejectId: string, FunctionName: string }
}

export interface MethodInstance {
    Name: string;
    TypeArguments: { [key: string]: TypeInstance }
    ParameterTypes: TypeInstance[];
}

export interface TypeInstance {
    Assembly: string;
    TypeName: string;
    TypeArguments: { [key: string]: TypeInstance };
}

export interface DotnetMethodArgumentsList {
    Argument1?: any;
    Argument2?: any;
}