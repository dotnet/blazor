using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Microsoft.AspNetCore.Blazor.Forms.Extensions
{
	/// <summary>
	/// </summary>
	public static class InputExtensions
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
		public static Microsoft.AspNetCore.Blazor.RenderFragment TextBoxFor<T,V>( 
			this Microsoft.AspNetCore.Blazor.Forms.Form<T> form, 
			Expression<Func<T, V>> Field, 
			object htmlAttributes = null )
		{
			var property = Internals.PropertyHelpers.GetProperty<T, V>(Field);
			string currentValue = form.ModelState.GetValue(property);

			return ( builder ) =>
			{
				int sequence = 1;

				builder.OpenElement(sequence++, "input");
				builder.AddAttribute(sequence++, "type", "text");
				builder.AddAttribute(sequence++, "name", property.Name);
				builder.AddAttribute(sequence++, "id", property.Name);
				builder.AddAttribute(sequence++, "value", (string)currentValue);

				builder.AddAttribute(sequence++, "onchange", 
					Microsoft.AspNetCore.Blazor.Components.BindMethods.GetEventHandlerValue<Microsoft.AspNetCore.Blazor.UIChangeEventArgs>(e =>
					{ 
						Console.WriteLine("onchange");
						form.ModelState.SetValue(property, e.Value);
					}
				));

				ExtensionsFunctions.WriteHtmlAttributes(builder, ref sequence, htmlAttributes);

				builder.CloseElement();
			};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="form"></param>
		/// <param name="Field"></param>
		/// <param name="htmlAttributes"></param>
		/// <returns></returns>
		public static Microsoft.AspNetCore.Blazor.RenderFragment CheckBoxFor<T, V>(
			this Microsoft.AspNetCore.Blazor.Forms.Form<T> form,
			Expression<Func<T, V>> Field,
			object htmlAttributes = null )
		{
			var property = Internals.PropertyHelpers.GetProperty<T, V>(Field);
			string currentValue = form.ModelState.GetValue(property)?.ToString();
			bool.TryParse(currentValue, out bool boolValue);

			return ( builder ) =>
			{
				int sequence = 1;

				builder.OpenElement(sequence++, "input");
				builder.AddAttribute(sequence++, "type", "checkbox");
				builder.AddAttribute(sequence++, "name", property.Name);
				builder.AddAttribute(sequence++, "id", property.Name);
				builder.AddAttribute(sequence++, "value", boolValue);
				builder.AddAttribute(sequence++, "onchange", new Action<UIChangeEventArgs>(( e ) => {
					form.ModelState.SetValue(property, e.Value);
				}));

				ExtensionsFunctions.WriteHtmlAttributes(builder, ref sequence, htmlAttributes);

				builder.CloseElement();
			};
		}


		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="form"></param>
		/// <param name="Field"></param>
		/// <param name="htmlAttributes"></param>
		/// <returns></returns>
		public static Microsoft.AspNetCore.Blazor.RenderFragment LabelFor<T, V>(
			this Microsoft.AspNetCore.Blazor.Forms.Form<T> form,
			Expression<Func<T, V>> Field,
			object htmlAttributes = null )
		{
			var property = Internals.PropertyHelpers.GetProperty<T, V>(Field);
			string currentValue = form.ModelState.GetValue(property);

			return ( builder ) =>
			{
				int sequence = 1;

				builder.OpenElement(sequence++, "label");
				builder.AddAttribute(sequence++, "for", property.Name);

				ExtensionsFunctions.WriteHtmlAttributes(builder, ref sequence, htmlAttributes);

				builder.AddContent(sequence++, ExtensionsFunctions.GetDisplayName(property));
				builder.CloseElement();
			};
		}
	}
}
