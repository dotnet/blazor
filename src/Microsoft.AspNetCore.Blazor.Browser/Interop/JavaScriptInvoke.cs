using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Blazor.Browser.Interop
{
    /// <summary>
    /// TODO
    /// </summary>
    public class JavaScriptInvoke
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="methodOptions"></param>
        /// <param name="methodArguments"></param>
        /// <returns></returns>
        public static string InvokeDotnetMethod(string methodOptions, string methodArguments)
        {
            Console.WriteLine(methodOptions);
            Console.WriteLine(methodArguments);
            var options = JsonUtil.Deserialize<MethodOptions>(methodOptions);
            var argumentDeserializer = GetOrCreateArgumentDeserializer(options);
            var invoker = GetOrCreateInvoker(options, argumentDeserializer);

            var result = invoker(methodArguments);
            if (options.Async != null && !(result is Task))
            {
                throw new InvalidOperationException($"'{options.Method.Name}' in '{options.Type.TypeName}' must return a Task.");
            }
            if (result is Task taskResult)
            {
                Console.WriteLine(result.GetType().FullName);
                taskResult.ContinueWith(task =>
                {
                    try
                    {
                        if (task.Status == TaskStatus.RanToCompletion)
                        {
                            if (task.GetType() == typeof(Task))
                            {
                                RegisteredFunction.Invoke<bool>(
                                    options.Async.FunctionName,
                                    options.Async.ResolveId);
                            }
                            else
                            {
                                var resultProperty = task.GetType()
                                        .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                                        .SingleOrDefault(m => m.Name == "ResultOnSuccess") ??
                                    task.GetType()
                                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                        .SingleOrDefault(m => m.Name == "Result");

                                var returnValue = resultProperty.GetValue(task);
                                RegisteredFunction.Invoke<bool>(
                                    options.Async.FunctionName,
                                    options.Async.ResolveId,
                                    returnValue);
                            }
                        }
                        else
                        {
                            RegisteredFunction.Invoke<bool>(
                                options.Async.FunctionName,
                                options.Async.RejectId);
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        throw;
                    }
                    finally
                    {
                        Console.WriteLine("Finally!");
                    }
                });
            }

            return JsonUtil.Serialize(result);
        }

        internal static MethodInfo ResolveMethod(MethodOptions options)
        {
            throw new NotImplementedException();
        }

        private static Func<string, object> GetOrCreateInvoker(MethodOptions options, Func<string, object[]> argumentDeserializer)
        {
            var method = options.GetMethodOrThrow();
            return (string args) => method.Invoke(null, argumentDeserializer(args));
        }

        private static Func<string, object[]> GetOrCreateArgumentDeserializer(MethodOptions options)
        {
            var info = options.GetMethodOrThrow();
            var argsClass = ArgumentList.GetArgumentClass(info.GetParameters().Select(p => p.ParameterType).ToArray());
            var deserializeMethod = ArgumentList.GetDeserializer(argsClass);
            var toParameterListMethod = argsClass.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => string.Equals(nameof(ArgumentList.ToParameterList), m.Name))
                .Single();

            return Deserialize;

            object[] Deserialize(string arguments)
            {
                var argsInstance = deserializeMethod(arguments);
                return argsInstance.ToParameterList();
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Task InvokeJavaScriptVoidFunctionAsync(string identifier, params object[] args)
        {
            var tcs = new TaskCompletionSource<int>();
            var argsJson = args.Select(JsonUtil.Serialize);
            var successId = Guid.NewGuid().ToString();
            var failureId = Guid.NewGuid().ToString();
            var asyncProtocol = JsonUtil.Serialize(new
            {
                Success = successId,
                Failure = failureId,
                Function = new MethodOptions
                {
                    Type = new TypeInstance
                    {
                        Assembly = typeof(JavaScriptInvoke).Assembly.FullName,
                        TypeName = typeof(JavaScriptInvoke).FullName
                    },
                    Method = new MethodInstance
                    {
                        Name = nameof(JavaScriptInvoke.InvokeTaskCallback),
                        ParameterTypes = new[]
                        {
                            new TypeInstance
                            {
                                Assembly = typeof(string).Assembly.FullName,
                                TypeName = typeof(string).FullName
                            },
                            new TypeInstance
                            {
                                Assembly = typeof(string).Assembly.FullName,
                                TypeName = typeof(string).FullName
                            }
                        }
                    }
                }
            });

            TrackedReference.Track(successId, (new Action(() =>
            {
                tcs.SetResult(0);
            })));

            TrackedReference.Track(failureId, (new Action<string>(r =>
            {
                tcs.SetException(new InvalidOperationException(r));
            })));

            var resultJson = RegisteredFunction.InvokeUnmarshalled<string>(
                "invokeWithJsonMarshallingAsync",
                argsJson.Prepend(asyncProtocol).Prepend(identifier).ToArray());

            return tcs.Task;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="identifier"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Task<TResult> InvokeJavaScriptFunctionAsync<TResult>(string identifier, params object[] args)
        {
            var tcs = new TaskCompletionSource<TResult>();
            var argsJson = args.Select(JsonUtil.Serialize);
            var successId = Guid.NewGuid().ToString();
            var failureId = Guid.NewGuid().ToString();
            var asyncProtocol = JsonUtil.Serialize(new
            {
                Success = successId,
                Failure = failureId,
                Function = new MethodOptions
                {
                    Type = new TypeInstance
                    {
                        Assembly = typeof(JavaScriptInvoke).Assembly.FullName,
                        TypeName = typeof(JavaScriptInvoke).FullName
                    },
                    Method = new MethodInstance
                    {
                        Name = nameof(JavaScriptInvoke.InvokeTaskCallback),
                        ParameterTypes = new[]
                        {
                            new TypeInstance
                            {
                                Assembly = typeof(string).Assembly.FullName,
                                TypeName = typeof(string).FullName
                            },
                            new TypeInstance
                            {
                                Assembly = typeof(string).Assembly.FullName,
                                TypeName = typeof(string).FullName
                            }
                        }
                    }
                }
            });

            TrackedReference.Track(successId, new Action<string>(r =>
            {
                var res = JsonUtil.Deserialize<TResult>(r);
                tcs.SetResult(res);
            }));

            TrackedReference.Track(failureId, (new Action<string>(r =>
            {
                tcs.SetException(new InvalidOperationException(r));
            })));

            var resultJson = RegisteredFunction.InvokeUnmarshalled<string>(
                "invokeWithJsonMarshallingAsync",
                argsJson.Prepend(asyncProtocol).Prepend(identifier).ToArray());

            return tcs.Task;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="id"></param>
        /// <param name="result"></param>
        public static void InvokeTaskCallback(string id, string result)
        {
            Console.WriteLine("Invoking task callback!");
            Console.WriteLine(id);
            Console.WriteLine(result ?? "(null)");
            var reference = TrackedReference.Get(id);
            var function = reference.TrackedInstance as Action<string>;
            function(result);
        }
    }
}
