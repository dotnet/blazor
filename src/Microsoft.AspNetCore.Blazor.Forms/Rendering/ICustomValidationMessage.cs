using Microsoft.AspNetCore.Blazor.RenderTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Blazor.Forms.Rendering
{
    /// <summary>
    /// </summary>
    public interface ICustomValidationMessage
    {
        /// <summary>
        /// </summary>
        void WriteValidationMessage(RenderTreeBuilder builder, string PropertyName, string Message, object htmlAttributes);
    }
}
