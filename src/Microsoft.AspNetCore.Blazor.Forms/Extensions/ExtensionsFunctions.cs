using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace Microsoft.AspNetCore.Blazor.Forms.Extensions
{
	/// <summary>
	/// 
	/// </summary>
  static public class ExtensionsFunctions
  {
		/// <summary>
		/// Get DisplayName of property
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public static string GetDisplayName( PropertyInfo property )
		{
			var display = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
			if (display != null) return display.Name;
			return property.Name;
		}

		/// <summary>
		/// Write attributes
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="sequence"></param>
		/// <param name="htmlAttributes"></param>
		public static void WriteHtmlAttributes( RenderTreeBuilder builder, ref int sequence, object htmlAttributes )
		{
            var attrs = Internals.Helpers.AnonymousObjectToHtmlAttributes(htmlAttributes);
            foreach (var attr in attrs)
                builder.AddAttribute(sequence++, attr.Key, attr.Value);
        }
	}
}
