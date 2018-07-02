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
        /// </summary>
        public static Microsoft.AspNetCore.Blazor.RenderFragment TextBoxFor<T, V>(
            this IForm<T> form,
            Expression<Func<T, V>> Field,
            object htmlAttributes = null) => form.ModelState.TextBoxFor(Field, htmlAttributes);

        /// <summary>
        /// </summary>
        public static Microsoft.AspNetCore.Blazor.RenderFragment TextBoxFor<T,V>( 
			this ModelStateDictionary<T> model, 
			Expression<Func<T, V>> Field, 
			object htmlAttributes = null )
		{
			var property = Extensions.PropertyHelpers.GetProperty(Field);
			string currentValue = model.GetValue(property)?.ToString();

			return ( builder ) =>
			{
				int sequence = 1;

				builder.OpenElement(sequence++, "input");
				builder.AddAttribute(sequence++, "type", "text");
				builder.AddAttribute(sequence++, "name", property.Name);
				builder.AddAttribute(sequence++, "id", property.Name);
				builder.AddAttribute(sequence++, "value", string.IsNullOrEmpty(currentValue) ? string.Empty : currentValue);
                builder.AddAttribute(sequence++, "autocomplete", "nope");

				builder.AddAttribute(sequence++, "onchange", 
					Microsoft.AspNetCore.Blazor.Components.BindMethods.GetEventHandlerValue<Microsoft.AspNetCore.Blazor.UIChangeEventArgs>(e =>
					{
                         model.SetValue(property.Name, property.PropertyType, e.Value);
					}
				));

                builder.WriteHtmlAttributes(ref sequence, htmlAttributes);

				builder.CloseElement();
			};
		}

        /// <summary>
		/// </summary>
		public static Microsoft.AspNetCore.Blazor.RenderFragment CheckBoxFor<T, V>(
			this IForm<T> form,
			Expression<Func<T, V>> Field,
			object htmlAttributes = null ) => form.ModelState.CheckBoxFor(Field, htmlAttributes);

		/// <summary>
		/// </summary>
		public static Microsoft.AspNetCore.Blazor.RenderFragment CheckBoxFor<T, V>(
			this ModelStateDictionary<T> model,
			Expression<Func<T, V>> Field,
			object htmlAttributes = null )
		{
			var property = Extensions.PropertyHelpers.GetProperty(Field);
			string currentValue = model.GetValue(property)?.ToString();
            if (bool.TryParse(currentValue, out bool boolValue) == false)
            {
                if (int.TryParse(currentValue, out int intValue) == true)
                {
                    boolValue = intValue == 0 ? false : true;
                }
            }

            //Console.WriteLine($"CheckBoxFor value={currentValue} {boolValue}");
        
            return ( builder ) =>
			{
				int sequence = 1;

				builder.OpenElement(sequence++, "input");
				builder.AddAttribute(sequence++, "type", "checkbox");
				builder.AddAttribute(sequence++, "name", property.Name);
				builder.AddAttribute(sequence++, "id", property.Name);
                if( boolValue == true) builder.AddAttribute(sequence++, "checked", "checked");
                builder.AddAttribute(sequence++, "onchange", new Action<UIChangeEventArgs>(( e ) => {
                    Console.WriteLine("checkbox=" + e.Value);
                    model.SetValue(property.Name, property.PropertyType, e.Value);
				}));

                builder.WriteHtmlAttributes(ref sequence, htmlAttributes);

				builder.CloseElement();
			};
		}

        /// <summary>
        /// </summary>
        public static Microsoft.AspNetCore.Blazor.RenderFragment LabelFor<T, V>(
            this IForm<T> form,
            Expression<Func<T, V>> Field,
            object htmlAttributes = null) => form.ModelState.LabelFor(Field, htmlAttributes);

        /// <summary>
        /// </summary>
        public static Microsoft.AspNetCore.Blazor.RenderFragment LabelFor<T, V>(
			this ModelStateDictionary<T> model,
			Expression<Func<T, V>> Field,
			object htmlAttributes = null,
            string DisplayName = null)
		{
			var property = Extensions.PropertyHelpers.GetProperty(Field);
			string currentValue = model.GetValue(property)?.ToString();

			return ( builder ) =>
			{
				int sequence = 1;

				builder.OpenElement(sequence++, "label");
				builder.AddAttribute(sequence++, "for", property.Name);

                builder.WriteHtmlAttributes(ref sequence, htmlAttributes);

                if(DisplayName == null) builder.AddContent(sequence++, ExtensionsFunctions.GetDisplayName(property));
				builder.CloseElement();
			};
		}
	}
}
