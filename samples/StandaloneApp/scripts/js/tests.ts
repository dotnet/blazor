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
};


interface IPlatform {
    invokeDotNetMethod<T>(methodOptions: MethodOptions, methodArgs?: DotnetMethodArgumentsList): T | null;
    invokeDotNetMethodAsync<T>(methodOptions: MethodOptions, methodArgs?: DotnetMethodArgumentsList): Promise<T | null>;
};

declare const Blazor: IBlazor;

function invokerTests() {
    Blazor.platform.invokeDotNetMethod(
        {
            Type: {
                Assembly: "BlazorApp.Client",
                TypeName: "BlazorApp.Client.Infrastructure.InvokerTests",
                TypeArguments: {}
            },
            Method: {
                Name: "ParameterlessMethod",
                TypeArguments: {},
                ParameterTypes: []
            }
        });

    let result = Blazor.platform.invokeDotNetMethod<{ IntegerValue: number, StringValue: string }>(
        {
            Type: {
                Assembly: "BlazorApp.Client",
                TypeName: "BlazorApp.Client.Infrastructure.InvokerTests",
                TypeArguments: {}
            },
            Method: {
                Name: "ParameterlessReturningMethod",
                TypeArguments: {},
                ParameterTypes: []
            }
        });

    if (result !== null) {
        console.log(`IntegerValue: '${result.IntegerValue}'`);
        console.log(`StringValue: '${result.StringValue}'`);
    }

    Blazor.platform.invokeDotNetMethod(
        {
            Type: {
                Assembly: "BlazorApp.Client",
                TypeName: "BlazorApp.Client.Infrastructure.InvokerTests",
                TypeArguments: {}
            },
            Method: {
                Name: "SingleParameterMethod",
                TypeArguments: {},
                ParameterTypes: [{
                    Assembly: "BlazorApp.Client",
                    TypeName: "BlazorApp.Client.Infrastructure.MethodParameter",
                    TypeArguments: {}
                }]
            }
        },
        { Argument1: { IntegerValue: 3, StringValue: "String 3" } });

    Blazor.platform.invokeDotNetMethodAsync(
        {
            Type: {
                Assembly: "BlazorApp.Client",
                TypeName: "BlazorApp.Client.Infrastructure.InvokerTests",
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
                Assembly: "BlazorApp.Client",
                TypeName: "BlazorApp.Client.Infrastructure.InvokerTests",
                TypeArguments: {}
            },
            Method: {
                Name: "SingleParameterMethodAsync",
                TypeArguments: {},
                ParameterTypes: [{
                    Assembly: "BlazorApp.Client",
                    TypeName: "BlazorApp.Client.Infrastructure.MethodParameter",
                    TypeArguments: {}
                }]
            }
        },
        { Argument1: { IntegerValue: 6, StringValue: "String 6" } })
        .then(() => console.log('After resolving task with parameter!'));

    let asyncPromise = Blazor.platform.invokeDotNetMethodAsync<{ IntegerValue: number, StringValue: string }>(
        {
            Type: {
                Assembly: "BlazorApp.Client",
                TypeName: "BlazorApp.Client.Infrastructure.InvokerTests",
                TypeArguments: {}
            },
            Method: {
                Name: "ParameterlessReturningMethodAsync",
                TypeArguments: {},
                ParameterTypes: []
            }
        }).then(res => {
            if (res !== null) {
                console.log(`IntegerValue: '${res.IntegerValue}'`);
                console.log(`StringValue: '${res.StringValue}'`);
            }
        });
}

setTimeout(invokerTests, 10000, []);