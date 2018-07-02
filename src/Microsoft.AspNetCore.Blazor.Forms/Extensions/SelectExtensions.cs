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
        public static Microsoft.AspNetCore.Blazor.RenderFragment DropDownListFor<T, V>(
          this IForm<T> form,
          Expression<Func<T, V>> Field,
          IEnumerable<Rendering.SelectListItem> selectList,
          object htmlAttributes = null) => form.ModelState.DropDownListFor(Field, selectList, htmlAttributes);

        /// <summary>
        /// </summary>
        public static Microsoft.AspNetCore.Blazor.RenderFragment DropDownListFor<T, V>(
                this ModelStateDictionary<T> model,
          Expression<Func<T, V>> Field,
          IEnumerable<Rendering.SelectListItem> selectList,
          object htmlAttributes = null)
        {
            var property = Extensions.PropertyHelper<T>.GetProperty<V>(Field);
            object currentValue = model.GetValue(property);

            return (builder) =>
            {
                int sequence = 1;

                builder.OpenElement(sequence++, "select");
                builder.AddAttribute(sequence++, "name", property.Name);
                builder.AddAttribute(sequence++, "id", property.Name);
                if (property.PropertyType.IsEnum && currentValue.GetType().IsEnum)
                {
                    builder.AddAttribute(sequence++, "value", (int)currentValue);
                }
                else
                {
                    builder.AddAttribute(sequence++, "value", currentValue?.ToString());
                }
                builder.AddAttribute(sequence++, "onchange", new Action<UIChangeEventArgs>((e) =>
                {
                    model.SetValue(property.Name, property.PropertyType, e.Value);
                }));

                builder.WriteHtmlAttributes(ref sequence, htmlAttributes);

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