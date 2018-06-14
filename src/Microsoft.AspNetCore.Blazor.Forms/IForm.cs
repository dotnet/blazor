using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace Microsoft.AspNetCore.Blazor.Forms
{
    ///// <summary>
    ///// 
    ///// </summary>
    //public interface IForm
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="handler"></param>
    //    /// <returns></returns>
    //    void OnChange(Action<object> handler);
    //}

    /// <summary>
    /// 
    /// </summary>
    public interface ICustomValidationMessage
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="PropertyName"></param>
		/// <param name="Message"></param>
		/// <param name="htmlAttributes"></param>
		void WriteValidationMessage( RenderTreeBuilder builder, string PropertyName, string Message, object htmlAttributes );
	}
}
