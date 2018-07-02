using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace Microsoft.AspNetCore.Blazor.Forms.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// </summary>
        public static Microsoft.AspNetCore.Blazor.RenderFragment ValidationMessageFor<T, V>(
          this IForm<T> form,
          Expression<Func<T, V>> Field,
          object htmlAttributes = null) => form.ModelState.ValidationMessageFor(Field, htmlAttributes, form as Rendering.ICustomValidationMessage);

        /// <summary>
        /// </summary>
        public static Microsoft.AspNetCore.Blazor.RenderFragment ValidationMessageFor<T, V>(
           this ModelStateDictionary<T> model,
          Expression<Func<T, V>> Field,
          object htmlAttributes = null,
          Rendering.ICustomValidationMessage customValidationMessage = null)
        {
            var property = Extensions.PropertyHelpers.GetProperty<T, V>(Field);
            var name = property.Name;

            return (builder) =>
            {
                string errorDescription = model.GetValidationResults()?
                    .Where(x => ((IEnumerable<string>)x.MemberNames).Contains(name))
                    .Select(x => x.ErrorMessage)
                    .FirstOrDefault();
                WriteMessage(customValidationMessage, builder, name, errorDescription, htmlAttributes);
            };
        }

        private static void WriteMessage(Rendering.ICustomValidationMessage customMessage,
            RenderTreeBuilder builder,
            string name, string errorDescription, object htmlAttributes = null)
        {
            if (string.IsNullOrEmpty(errorDescription) == false)
            {
                if (customMessage != null)
                {
                    customMessage.WriteValidationMessage(builder, name, errorDescription, htmlAttributes);
                }
                else
                {
                    int sequence = 1;

                    builder.OpenElement(sequence++, "p");
                    builder.AddContent(sequence++, errorDescription);

                    builder.WriteHtmlAttributes(ref sequence, htmlAttributes);

                    builder.CloseElement();
                }
            }
        }
    }
}