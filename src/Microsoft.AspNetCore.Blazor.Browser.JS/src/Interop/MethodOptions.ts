export interface MethodOptions {
    type: TypeInstance;
    method: MethodInstance;
    async?: { resolveId: string, rejectId: string, functionName: string }
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