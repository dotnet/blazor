interface MethodOptions {
    Type: TypeInstance;
    Method: MethodInstance;
    Async?: { ResolveId: string, RejectId: string, FunctionName: string }
}

interface MethodInstance {
    Name: string;
    TypeArguments: { [key: string]: TypeInstance }
    ParameterTypes: TypeInstance[];
}

interface TypeInstance {
    Assembly: string;
    TypeName: string;
    TypeArguments: { [key: string]: TypeInstance };
}

interface DotnetMethodArgumentsList {
    Argument1?: any;
    Argument2?: any;
}

interface IBlazor {
  platform: IPlatform
  registerFunction: (name: string, implementation: Function) => void;
};


interface IPlatform {
    invokeDotNetMethod<T>(methodOptions: MethodOptions, methodArgs?: DotnetMethodArgumentsList): T | null;
    invokeDotNetMethodAsync<T>(methodOptions: MethodOptions, methodArgs?: DotnetMethodArgumentsList): Promise<T | null>;
};

declare const Blazor: IBlazor;

function myAsyncFunction() {
  return Promise.resolve({ integerValue: 8, stringValue: "String 8" });
}

function registerMyAppFunctions(blazor: IBlazor) {
  blazor.registerFunction("MyAsyncFunc", myAsyncFunction);
}

function invokerTests() {
    console.log('Starting invocations.');
    console.log('Invoking parameterless method.');
    Blazor.platform.invokeDotNetMethod(
        {
            Type: {
                Assembly: "StandaloneApp",
                TypeName: "StandaloneApp.Infrastructure.InvokerTests",
                TypeArguments: {}
            },
            Method: {
                Name: "ParameterlessMethod",
                TypeArguments: {},
                ParameterTypes: []
            }
        });

    console.log('Invoking parameterless value returning method.');
    let result = Blazor.platform.invokeDotNetMethod<{ integerValue: number, stringValue: string }>(
        {
            Type: {
                Assembly: "StandaloneApp",
                TypeName: "StandaloneApp.Infrastructure.InvokerTests",
                TypeArguments: {}
            },
            Method: {
                Name: "ParameterlessReturningMethod",
                TypeArguments: {},
                ParameterTypes: []
            }
        });

    if (result !== null) {
        console.log(`integerValue: '${result.integerValue}'`);
        console.log(`stringValue: '${result.stringValue}'`);
    }

    Blazor.platform.invokeDotNetMethod(
        {
            Type: {
                Assembly: "StandaloneApp",
                TypeName: "StandaloneApp.Infrastructure.InvokerTests",
                TypeArguments: {}
            },
            Method: {
                Name: "SingleParameterMethod",
                TypeArguments: {},
                ParameterTypes: [{
                    Assembly: "StandaloneApp",
                    TypeName: "StandaloneApp.Infrastructure.MethodParameter",
                    TypeArguments: {}
                }]
            }
        },
        { Argument1: { integerValue: 3, stringValue: "String 3" } });

    Blazor.platform.invokeDotNetMethodAsync(
        {
            Type: {
                Assembly: "StandaloneApp",
                TypeName: "StandaloneApp.Infrastructure.InvokerTests",
                TypeArguments: {}
            },
            Method: {
                Name: "ParameterlessMethodAsync",
                TypeArguments: {},
                ParameterTypes: []
            }
        }).then(() => console.log('After resolving task'));

    Blazor.platform.invokeDotNetMethodAsync(
        {
            Type: {
                Assembly: "StandaloneApp",
                TypeName: "StandaloneApp.Infrastructure.InvokerTests",
                TypeArguments: {}
            },
            Method: {
                Name: "SingleParameterMethodAsync",
                TypeArguments: {},
                ParameterTypes: [{
                    Assembly: "StandaloneApp",
                    TypeName: "StandaloneApp.Infrastructure.MethodParameter",
                    TypeArguments: {}
                }]
            }
        },
        { Argument1: { integerValue: 6, stringValue: "String 6" } })
        .then(() => console.log('After resolving task with parameter!'));

    let asyncPromise = Blazor.platform.invokeDotNetMethodAsync<{ integerValue: number, stringValue: string }>(
        {
            Type: {
                Assembly: "StandaloneApp",
                TypeName: "StandaloneApp.Infrastructure.InvokerTests",
                TypeArguments: {}
            },
            Method: {
                Name: "ParameterlessReturningMethodAsync",
                TypeArguments: {},
                ParameterTypes: []
            }
        }).then(res => {
            if (res !== null) {
                console.log(`integerValue: '${res.integerValue}'`);
                console.log(`stringValue: '${res.stringValue}'`);
            }
        });
}