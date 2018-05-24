namespace Microsoft.AspNetCore.Blazor.Browser.Interop
{
    // Represents the information to invoke a JavaScript callback as part of invoking
    // an asynchronous dotnet function.
    internal class JavaScriptAsync
    {
        // The id of the specific JavaScript callback that will
        // handle the async response. This callback must have the
        // signature (invocationResult: InvocationResult) -> void
        public string CallbackId { get; set; }

        // The name of the function that handles the callbacks.
        // This function name must have the signature
        // (callbackId: string, invocationResult: invocationResult) -> void
        public string FunctionName { get; set; }
    }
}
