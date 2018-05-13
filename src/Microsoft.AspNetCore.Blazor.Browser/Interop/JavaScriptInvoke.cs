using System;
using System.Collections.Generic;
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
                taskResult.ContinueWith(task =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        if (task.GetType().GetGenericTypeDefinition() == typeof(Task<>))
                        {
                            var returnValue = task.GetType().GetProperty("Result").GetValue(task);
                            RegisteredFunction.Invoke<bool>(
                                options.Async.FunctionName,
                                options.Async.ResolveId,
                                returnValue);
                        }
                        else
                        {
                            RegisteredFunction.Invoke<bool>(
                                options.Async.FunctionName,
                                options.Async.ResolveId);
                        }
                    }
                    else
                    {
                        RegisteredFunction.Invoke<bool>(
                            options.Async.FunctionName,
                            options.Async.ResolveId);
                    }
                });
            }

            return JsonUtil.Serialize(result);
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
            var deserializeMethod = argsClass.GetMethod(nameof(ArgumentList.JsonDeserialize));
            var toParameterListMethod = argsClass.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => string.Equals(nameof(ArgumentList.ToParameterList), m.Name))
                .Single();

            return Deserialize;

            object[] Deserialize(string arguments)
            {
                var argsInstance = deserializeMethod.Invoke(null, new object[] { arguments });
                return (object[])toParameterListMethod.Invoke(argsInstance, Array.Empty<object>());
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
            var reference = TrackedReference.Get(id);
            var function = reference.TrackedInstance as Action<string>;
            function(result);
        }
    }

    internal class ArgumentList
    {
        public static ArgumentList Instance { get; } = new ArgumentList();

        public static Type GetArgumentClass(Type[] arguments)
        {
            switch (arguments.Length)
            {
                case 0:
                    return typeof(ArgumentList);
                case 1:
                    return typeof(ArgumentList<>).MakeGenericType(arguments);
                case 2:
                    return typeof(ArgumentList<,>).MakeGenericType(arguments);
                case 3:
                    return typeof(ArgumentList<,,>).MakeGenericType(arguments);
                case 4:
                    return typeof(ArgumentList<,,,>).MakeGenericType(arguments);
                case 5:
                    return typeof(ArgumentList<,,,,>).MakeGenericType(arguments);
                case 6:
                    return typeof(ArgumentList<,,,,,>).MakeGenericType(arguments);
                case 7:
                    return typeof(ArgumentList<,,,,,,>).MakeGenericType(arguments);
                default:
                    throw new InvalidOperationException("Unsupported number of arguments");
            }
        }

        public static ArgumentList JsonDeserialize(string item) => Instance;

        public object[] ToParameterList() => Array.Empty<object>();
    }

    internal class ArgumentList<T1>
    {
        public T1 Argument1 { get; set; }

        public static ArgumentList<T1> JsonDeserialize(string item) =>
            JsonUtil.Deserialize<ArgumentList<T1>>(item);

        public object[] ToParameterList() => new object[] { Argument1 };
    }

    internal class ArgumentList<T1, T2>
    {
        public T1 Argument1 { get; set; }
        public T2 Argument2 { get; set; }

        public static ArgumentList<T1, T2> JsonDeserialize(string item) =>
            JsonUtil.Deserialize<ArgumentList<T1, T2>>(item);

        public object[] ToParameterList() => new object[] { Argument1, Argument2 };
    }

    internal class ArgumentList<T1, T2, T3>
    {
        public T1 Argument1 { get; set; }
        public T2 Argument2 { get; set; }
        public T3 Argument3 { get; set; }
    }

    internal class ArgumentList<T1, T2, T3, T4>
    {
        public T1 Argument1 { get; set; }
        public T2 Argument2 { get; set; }
        public T3 Argument3 { get; set; }
        public T4 Argument4 { get; set; }
    }

    internal class ArgumentList<T1, T2, T3, T4, T5>
    {
        public T1 Argument1 { get; set; }
        public T2 Argument2 { get; set; }
        public T3 Argument3 { get; set; }
        public T4 Argument4 { get; set; }
        public T5 Argument5 { get; set; }
    }
    internal class ArgumentList<T1, T2, T3, T4, T5, T6>
    {
        public T1 Argument1 { get; set; }
        public T2 Argument2 { get; set; }
        public T3 Argument3 { get; set; }
        public T4 Argument4 { get; set; }
        public T5 Argument5 { get; set; }
        public T6 Argument6 { get; set; }
    }
    internal class ArgumentList<T1, T2, T3, T4, T5, T6, T7>
    {
        public T1 Argument1 { get; set; }
        public T2 Argument2 { get; set; }
        public T3 Argument3 { get; set; }
        public T4 Argument4 { get; set; }
        public T5 Argument5 { get; set; }
        public T6 Argument6 { get; set; }
        public T7 Argument7 { get; set; }

        public ArgumentList<T1, T2, T3, T4, T5, T6, T7> JsonDeserialize(string item) =>
            JsonUtil.Deserialize<ArgumentList<T1, T2, T3, T4, T5, T6, T7>>(item);
    }

    internal class TypeInstance
    {
        public TypeInstance()
        {
        }

        public string Assembly { get; set; }
        public string TypeName { get; set; }
        public IDictionary<string, TypeInstance> TypeArguments { get; set; }

        internal Type GetTypeOrThrow()
        {
            return Type.GetType($"{TypeName}, {Assembly}", throwOnError: true);
        }
    }

    internal class MethodInstance
    {
        public MethodInstance()
        {
        }

        public string Name { get; set; }
        public IDictionary<string, TypeInstance> TypeArguments { get; set; }
        public TypeInstance[] ParameterTypes { get; set; }

        internal MethodInfo GetMethodOrThrow(Type type)
        {
            var result = type.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(m => string.Equals(m.Name, Name, StringComparison.Ordinal)).SingleOrDefault();
            return result ?? throw new InvalidOperationException($"Couldn't found a method with name '{Name}' in '{type.FullName}'.");
        }
    }

    internal class MethodOptions
    {
        public MethodOptions()
        {
        }

        public TypeInstance Type { get; set; }
        public MethodInstance Method { get; set; }
        public Async Async { get; set; }

        internal MethodInfo GetMethodOrThrow()
        {
            var type = Type.GetTypeOrThrow();
            var method = Method.GetMethodOrThrow(type);

            return method;
        }
    }

    internal class Async
    {
        public Async()
        {
        }

        public string ResolveId { get; set; }
        public string RejectId { get; set; }
        public string FunctionName { get; set; }
    }

    internal class TrackedReference
    {
        private TrackedReference(string id, object reference)
        {
            Id = id;
            TrackedInstance = reference;
        }
        private static IDictionary<string, TrackedReference> References { get; } =
            new Dictionary<string, TrackedReference>();

        public string Id { get; }

        public object TrackedInstance { get; }

        public static void Track(string id, object reference)
        {
            var trackedRef = new TrackedReference(id, reference);
            if (References.ContainsKey(id))
            {
                throw new InvalidOperationException($"An element with id '{id}' is already being tracked.");
            }

            References.Add(id, trackedRef);
        }

        public static void Untrack(string id)
        {
            if (!References.ContainsKey(id))
            {
                throw new InvalidOperationException($"An element with id '{id}' is not being tracked.");
            }
        }

        public static TrackedReference Get(string id)
        {
            if (!References.ContainsKey(id))
            {
                throw new InvalidOperationException($"An element with id '{id}' is not being tracked.");
            }

            return References[id];
        }
    }
}
