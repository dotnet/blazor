interface MethodOptions {
  type: TypeInstance;
  method: MethodInstance;
  async?: { resolveId: string, rejectId: string, functionName: string }
}

interface MethodInstance {
  name: string;
  typeArguments?: { [key: string]: TypeInstance }
  parameterTypes?: TypeInstance[];
}

interface TypeInstance {
  assembly: string;
  typeName: string;
  typeArguments?: { [key: string]: TypeInstance };
}

interface DotnetMethodArgumentsList {
  argument1?: any;
  argument2?: any;
  argument3?: any;
  argument4?: any;
  argument5?: any;
  argument6?: any;
  argument7?: any;
  argument8?: any;
}

interface IPlatform {
  invokeDotNetMethod(methodOptions: MethodOptions, ...arguments: any[]): any;
  invokeDotNetMethodAsync(methodOptions: MethodOptions, ...arguments: any[]): Promise<any>;
}

interface IBlazor {
  platform: IPlatform;
  registerFunction(id: string, implementation: Function): void;
}

declare var Blazor: IBlazor;

// We'll store the results from the tests here
let results = new Map<string, any>();

function createMethodOptions(methodName: string): MethodOptions {
  return {
    type: {
      assembly: "BasicTestApp",
      typeName: "BasicTestApp.InteropTest.JavaScriptInterop"
    },
    method: {
      name: methodName
    }
  };
}

function createArgumentList(argumentNumber: number): any[] {
  const array: any[] = new Array(argumentNumber);
  if (argumentNumber === 0) {
    return undefined;
  }
  for (var i = 0; i < argumentNumber; i++) {
    switch (i) {
      case 0:
        array[i] = {
          id: argumentNumber,
          isValid: argumentNumber % 2 === 0,
          data: {
            source: `Some random text with at least ${argumentNumber} characters`,
            start: argumentNumber,
            length: argumentNumber
          }
        };
        break;
      case 1:
        array[i] = argumentNumber;
        break;
      case 2:
        array[i] = argumentNumber * 2;
        break;
      case 3:
        array[i] = argumentNumber * 4;
        break;
      case 4:
        array[i] = argumentNumber * 8;
        break;
      case 5:
        array[i] = argumentNumber + 0.25;
        break;
      case 6:
        array[i] = Array.apply(null, Array(argumentNumber)).map((v, i) => i + 0.5);
        break;
      case 7:
        array[i] = {
          source: `Some random text with at least ${i} characters`,
          start: argumentNumber + 1,
          length: argumentNumber + 1
        }
        break;
      default:
        console.log(i);
        throw new Error("Invalid argument count!");
    }
  }

  return array;
}

Blazor.registerFunction('BasicTestApp.Interop.InvokeDotNetInteropMethodsAsync', invokeDotNetInteropMethodsAsync);
Blazor.registerFunction('BasicTestApp.Interop.CollectResults', collectInteropResults);

Blazor.registerFunction('BasicTestApp.Interop.FunctionThrows', functionThrowsException);
Blazor.registerFunction('BasicTestApp.Interop.AsyncFunctionThrowsSyncException', asyncFunctionThrowsSyncException);
Blazor.registerFunction('BasicTestApp.Interop.AsyncFunctionThrowsAsyncException', asyncFunctionThrowsAsyncException);

function functionThrowsException() {
  throw new Error('Function threw an exception!');
}

function asyncFunctionThrowsSyncException() {
  throw new Error('Function threw a sync exception!');
}

function asyncFunctionThrowsAsyncException() {
  return new Promise((resolve, reject) => {
    setTimeout(() => reject(new Error('Function threw an async exception!')), 3000);
  });
}

function collectInteropResults() {
  let result = {};
  for (let [key, value] of results) {
    result[key] = btoa(JSON.stringify(value));
  }

  return result;
}

async function invokeDotNetInteropMethodsAsync(): Promise<void> {
  console.log('Invoking void sync methods.');
  Blazor.platform.invokeDotNetMethod(createMethodOptions('VoidParameterless'));
  Blazor.platform.invokeDotNetMethod(createMethodOptions('VoidWithOneParameter'), ...createArgumentList(1));
  Blazor.platform.invokeDotNetMethod(createMethodOptions('VoidWithTwoParameters'), ...createArgumentList(2));
  Blazor.platform.invokeDotNetMethod(createMethodOptions('VoidWithThreeParameters'), ...createArgumentList(3));
  Blazor.platform.invokeDotNetMethod(createMethodOptions('VoidWithFourParameters'), ...createArgumentList(4));
  Blazor.platform.invokeDotNetMethod(createMethodOptions('VoidWithFiveParameters'), ...createArgumentList(5));
  Blazor.platform.invokeDotNetMethod(createMethodOptions('VoidWithSixParameters'), ...createArgumentList(6));
  Blazor.platform.invokeDotNetMethod(createMethodOptions('VoidWithSevenParameters'), ...createArgumentList(7));
  Blazor.platform.invokeDotNetMethod(createMethodOptions('VoidWithEightParameters'), ...createArgumentList(8));

  console.log('Invoking returning sync methods.');
  results.set('result1', Blazor.platform.invokeDotNetMethod(createMethodOptions('ReturnArray')));
  results.set('result2', Blazor.platform.invokeDotNetMethod(createMethodOptions('EchoOneParameter'), ...createArgumentList(1)));
  results.set('result3', Blazor.platform.invokeDotNetMethod(createMethodOptions('EchoTwoParameters'), ...createArgumentList(2)));
  results.set('result4', Blazor.platform.invokeDotNetMethod(createMethodOptions('EchoThreeParameters'), ...createArgumentList(3)));
  results.set('result5', Blazor.platform.invokeDotNetMethod(createMethodOptions('EchoFourParameters'), ...createArgumentList(4)));
  results.set('result6', Blazor.platform.invokeDotNetMethod(createMethodOptions('EchoFiveParameters'), ...createArgumentList(5)));
  results.set('result7', Blazor.platform.invokeDotNetMethod(createMethodOptions('EchoSixParameters'), ...createArgumentList(6)));
  results.set('result8', Blazor.platform.invokeDotNetMethod(createMethodOptions('EchoSevenParameters'), ...createArgumentList(7)));
  results.set('result9', Blazor.platform.invokeDotNetMethod(createMethodOptions('EchoEightParameters'), ...createArgumentList(8)));

  console.log('Invoking void async methods.');
  await Blazor.platform.invokeDotNetMethodAsync(createMethodOptions('VoidParameterlessAsync'));
  await Blazor.platform.invokeDotNetMethodAsync(createMethodOptions('VoidWithOneParameterAsync'), ...createArgumentList(1));
  await Blazor.platform.invokeDotNetMethodAsync(createMethodOptions('VoidWithTwoParametersAsync'), ...createArgumentList(2));
  await Blazor.platform.invokeDotNetMethodAsync(createMethodOptions('VoidWithThreeParametersAsync'), ...createArgumentList(3));
  await Blazor.platform.invokeDotNetMethodAsync(createMethodOptions('VoidWithFourParametersAsync'), ...createArgumentList(4));
  await Blazor.platform.invokeDotNetMethodAsync(createMethodOptions('VoidWithFiveParametersAsync'), ...createArgumentList(5));
  await Blazor.platform.invokeDotNetMethodAsync(createMethodOptions('VoidWithSixParametersAsync'), ...createArgumentList(6));
  await Blazor.platform.invokeDotNetMethodAsync(createMethodOptions('VoidWithSevenParametersAsync'), ...createArgumentList(7));
  await Blazor.platform.invokeDotNetMethodAsync(createMethodOptions('VoidWithEightParametersAsync'), ...createArgumentList(8));

  console.log('Invoking returning async methods.');
  results.set('result1Async', await Blazor.platform.invokeDotNetMethodAsync(createMethodOptions('ReturnArrayAsync')));
  results.set('result2Async', await Blazor.platform.invokeDotNetMethodAsync(createMethodOptions('EchoOneParameterAsync'), ...createArgumentList(1)));
  results.set('result3Async', await Blazor.platform.invokeDotNetMethodAsync(createMethodOptions('EchoTwoParametersAsync'), ...createArgumentList(2)));
  results.set('result4Async', await Blazor.platform.invokeDotNetMethodAsync(createMethodOptions('EchoThreeParametersAsync'), ...createArgumentList(3)));
  results.set('result5Async', await Blazor.platform.invokeDotNetMethodAsync(createMethodOptions('EchoFourParametersAsync'), ...createArgumentList(4)));
  results.set('result6Async', await Blazor.platform.invokeDotNetMethodAsync(createMethodOptions('EchoFiveParametersAsync'), ...createArgumentList(5)));
  results.set('result7Async', await Blazor.platform.invokeDotNetMethodAsync(createMethodOptions('EchoSixParametersAsync'), ...createArgumentList(6)));
  results.set('result8Async', await Blazor.platform.invokeDotNetMethodAsync(createMethodOptions('EchoSevenParametersAsync'), ...createArgumentList(7)));
  results.set('result9Async', await Blazor.platform.invokeDotNetMethodAsync(createMethodOptions('EchoEightParametersAsync'), ...createArgumentList(8)));

  console.log('Invoking methods that throw exceptions');
  try {
    Blazor.platform.invokeDotNetMethod(createMethodOptions('ThrowException'))
  } catch (e) {
    results.set('ThrowException', e.message);
  }
  try {
    await Blazor.platform.invokeDotNetMethodAsync(createMethodOptions('AsyncThrowSyncException'))
  } catch (e) {
    results.set('AsyncThrowSyncException', e.message);
  }

  try {
    await Blazor.platform.invokeDotNetMethodAsync(createMethodOptions('AsyncThrowAsyncException'))
  } catch (e) {
    results.set('AsyncThrowAsyncException', e.message);
  }

  console.log('Done invoking interop methods');
}
