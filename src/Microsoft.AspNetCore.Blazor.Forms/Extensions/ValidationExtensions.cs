using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Microsoft.AspNetCore.Blazor.Forms.Extensions
{
	/// <summary>
	/// 
	/// </summary>
	public static class ValidationExtensions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="form"></param>
		/// <param name="Field"></param>
		/// <param name="htmlAttributes"></param>
		/// <returns></returns>
		public static Microsoft.AspNetCore.Blazor.RenderFragment ValidationMessageFor<T,V>( 
			this Microsoft.AspNetCore.Blazor.Forms.Form<T> form, 
			Expression<Func<T, V>> Field,
			object htmlAttributes = null )
		{
			var property = Internals.PropertyHelpers.GetProperty<T,V>(Field);
			var name = property.Name;

			return ( builder ) =>
			{
				string errorDescription = form.GetValidationResults()?
							.Where(x => ((IEnumerable<string>)x.MemberNames).Contains(name))
							.Select(x => x.ErrorMessage)
							.FirstOrDefault();
				if (string.IsNullOrEmpty(errorDescription) == false)
				{
					var customMessage = form as ICustomValidationMessage;
					if (customMessage != null)
					{
						customMessage.WriteValidationMessage(builder, name, errorDescription, htmlAttributes);
					}
					else
					{
						int sequence = 1;

						builder.OpenElement(sequence++, "p");
						builder.AddContent(sequence++, errorDescription);

						ExtensionsFunctions.WriteHtmlAttributes(builder, ref sequence, htmlAttributes);

						builder.CloseElement();
					}
				}
			};
		}
	}
}