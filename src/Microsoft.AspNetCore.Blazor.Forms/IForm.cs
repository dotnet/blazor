using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace Microsoft.AspNetCore.Blazor.Forms
{
	//public interface IForm
	//{
	//	RenderTreeFrame onchange( Action<object> handler );
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
