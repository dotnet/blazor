using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Microsoft.AspNetCore.Blazor.Forms.Extensions
{
	/// <summary>
	/// 
	/// </summary>
	public static class DropZoneExtensions
	{
		/// <summary>
		/// 
		/// </summary>
		public class DropZoneOptions
		{
			/// <summary>
			/// 
			/// </summary>
			public string PostUrl { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public Action<string, Components.DropZone.FileEventArgs> OnFileAdded { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public Action<string, Components.DropZone.FileEventArgs> OnFileRemoved { get; set; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="form"></param>
		/// <param name="Field"></param>
		/// <param name="options"></param>
		/// <param name="htmlAttributes"></param>
		/// <returns></returns>
		public static Microsoft.AspNetCore.Blazor.RenderFragment DropZoneFor<T, V>(
			this Microsoft.AspNetCore.Blazor.Forms.Form<T> form,
			Expression<Func<T, V>> Field,
			DropZoneOptions options,
			object htmlAttributes = null )
		{
			var property = Internals.PropertyHelpers.GetProperty<T, V>(Field);
			string currentValue = form.ModelState.GetValue(property);

			return ( builder ) =>
			{
				int sequence = 1;

				builder.OpenComponent<Components.DropZone>(sequence++);
				builder.AddAttribute(sequence++, "Name", property.Name);
				builder.AddAttribute(sequence++, "Id", property.Name);
				if (options?.PostUrl != null)
				{
					builder.AddAttribute(sequence++, "Url", options.PostUrl);
				}
				//if (options?.OnFileAdded != null)
				//{
				//	builder.AddAttribute(sequence++, "OnFileAdded", args =>
				//		{
				//			var customArgs = args as UICustomEventArgs;
				//			var fileArgs = JsonUtil.Deserialize<Components.DropZone.FileEventArgs>((string)customArgs.Value);
				//			options.OnFileAdded.Invoke( property.Name, fileArgs);
				//		});
				//}
				//if (options?.OnFileRemoved != null)
				//{
				//	builder.AddAttribute(sequence++, "OnFileRemoved", args =>
				//	{
				//		var customArgs = args as UICustomEventArgs;
				//		var fileArgs = JsonUtil.Deserialize<Components.DropZone.FileEventArgs>((string)customArgs.Value);
				//		options.OnFileRemoved.Invoke(property.Name, fileArgs);
				//	});
				//}
				ExtensionsFunctions.WriteHtmlAttributes(builder, ref sequence, htmlAttributes);
				builder.CloseComponent();
			};
		}
	}
}
