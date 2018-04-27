using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Microsoft.AspNetCore.Blazor.Forms.Extensions
{
	/// <summary>
	/// </summary>
	public static class SelectExtensions
	{
		/// <summary>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="form"></param>
		/// <param name="Field"></param>
		/// <param name="selectList"></param>
		/// <param name="htmlAttributes"></param>
		/// <returns></returns>
		public static Microsoft.AspNetCore.Blazor.RenderFragment DropDownListFor<T, V>(
			this Microsoft.AspNetCore.Blazor.Forms.Form<T> form,
			Expression<Func<T, V>> Field,
			IEnumerable<SelectListItem> selectList,
			object htmlAttributes = null )
		{
			var property = Internals.PropertyHelpers.GetProperty<T, V>(Field);
			string currentValue = form.ModelState.GetValue(property);

			return ( builder ) =>
			{
				int sequence = 1;

				builder.OpenElement(sequence++, "select");
				builder.AddAttribute(sequence++, "name", property.Name);
				builder.AddAttribute(sequence++, "id", property.Name);
				builder.AddAttribute(sequence++, "value", (string)currentValue);
				builder.AddAttribute(sequence++, "onchange", (UIChangeEventHandler)(( e ) => {
					form.ModelState.SetValue(property, e.Value);
				}));

				ExtensionsFunctions.WriteHtmlAttributes(builder, ref sequence, htmlAttributes);

				foreach (var item in selectList)
				{
					builder.OpenElement(sequence++, "option");
					builder.AddAttribute(sequence++, "value", item.Value);
					if (item.Disabled == true) builder.AddAttribute(sequence++, "disabled", item.Disabled);
					if (item.Selected == true) builder.AddAttribute(sequence++, "selected", item.Selected);
					builder.AddContent(sequence++, item.Text);
					builder.CloseElement();
				}
				builder.CloseElement();
			};
		}
	}
}
