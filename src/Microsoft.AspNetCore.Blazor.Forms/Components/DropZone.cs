using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace Microsoft.AspNetCore.Blazor.Forms.Components
{
	/// <summary>
	/// 
	/// </summary>
	public class DropZone : Blazor.Components.BlazorComponent
	{
		[Blazor.Components.Inject]
		private System.Net.Http.HttpClient httpClient { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string AuthorizationHeader { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public UIEventHandler OnFileAdded { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public UIEventHandler OnFileRemoved { get; set; }

		/// <inheritdoc />
		protected override void BuildRenderTree( RenderTreeBuilder builder )
		{
			int sequence = 1;

			httpClient.DefaultRequestHeaders.TryGetValues("Authorization", out IEnumerable<string> Items);

			//builder.AddAttribute(0, "data-url", Url);
			//builder.AddAttribute(0, "data-authorization", Items?.FirstOrDefault());
			//if (OnFileAdded != null) builder.AddAttribute(0, "onfileadded", OnFileAdded);

			builder.OpenElement(sequence++, "div");
			builder.AddAttribute(sequence++, "data-authorization", Items?.FirstOrDefault());

			builder.AddAttribute(sequence++, "class", "dropzone needsclick");
				builder.OpenElement(sequence++, "div");
					builder.AddAttribute(sequence++, "class", "dz-message needsclick");
					builder.AddContent(sequence++, "Drop files ");
					builder.OpenElement(sequence++, "span");
						builder.AddAttribute(sequence++, "class", "dz-note needsclick");
					builder.CloseElement();
				builder.CloseElement();
			builder.CloseElement();

			//<dropzone>
			//	<div class="dropzone needsclick">
			//		<div class="dz-message needsclick">
			//			Drop files <b>here</b> or <b>click</b> to upload.<br />
			//			<span class="dz-note needsclick">
			//				(This is just a demo dropzone. Selected files are <strong>not</strong> actually uploaded.)
			//			</span>
			//		</div>
			//	</div>
			//</dropzone>

			base.BuildRenderTree(builder);
		}

		/// <summary>
		/// 
		/// </summary>
		public class FileEventArgs
		{
			/// <summary>
			/// 
			/// </summary>
			public string FileName { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public int Size { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public System.Guid Guid { get; set; }
		}
	}
}
