using System;

namespace Microsoft.AspNetCore.Blazor.Routing
{
    /// <summary>
    /// Represents when a route could not be found.
    /// </summary>
    [Serializable]
    public class RouteNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.AspNetCore.Blazor.Routing.RouteNotFoundException"></see> class.
        /// </summary>
        public RouteNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.AspNetCore.Blazor.Routing.RouteNotFoundException"></see> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public RouteNotFoundException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.AspNetCore.Blazor.Routing.RouteNotFoundException"></see> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public RouteNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
