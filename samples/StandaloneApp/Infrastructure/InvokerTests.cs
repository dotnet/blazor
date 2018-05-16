using System;
using System.Threading.Tasks;

namespace StandaloneApp.Infrastructure
{
    public class InvokerTests
    {
        public static void ParameterlessMethod() { Console.WriteLine(nameof(ParameterlessMethod)); }

        public static MethodParameter ParameterlessReturningMethod() { return new MethodParameter { IntegerValue = 5, StringValue = "string 5" }; }

        public static void SingleParameterMethod(MethodParameter p)
        {
            Console.WriteLine($"{nameof(SingleParameterMethod)} - {p.IntegerValue}, {p.StringValue}");
        }

        public static Task ParameterlessMethodAsync() { Console.WriteLine(nameof(ParameterlessMethodAsync)); return Task.CompletedTask; }

        public static Task<MethodParameter> ParameterlessReturningMethodAsync() { return Task.FromResult(new MethodParameter { IntegerValue = 10, StringValue = "string 10" }); }

        public static Task SingleParameterMethodAsync(MethodParameter p)
        {
            Console.WriteLine($"{nameof(SingleParameterMethod)} - {p.IntegerValue}, {p.StringValue}");
            return Task.CompletedTask;
        }
    }

    public class MethodParameter
    {
        public int IntegerValue { get; set; }
        public string StringValue { get; set; }
    }
}
