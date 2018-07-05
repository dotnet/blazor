using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Blazor.Forms.Rendering
{
	/// <summary>
	/// This class is typically rendered as an HTML <code>&lt;option&gt;</code> element with the specified
	/// attribute values.
	/// </summary>
	public class SelectListItem
	{
		/// <summary>
		/// Gets or sets a value that indicates whether this <see cref="SelectListItem"/> is disabled.
		/// This property is typically rendered as a <code>disabled="disabled"</code> attribute in the HTML
		/// <code>&lt;option&gt;</code> element.
		/// </summary>
		public bool Disabled { get; set; }

		///// <summary>
		///// Represents the optgroup HTML element this item is wrapped into.
		///// In a select list, multiple groups with the same name are supported.
		///// They are compared with reference equality.
		///// </summary>
		//public SelectListGroup Group { get; set; }

		/// <summary>
		/// Gets or sets a value that indicates whether this <see cref="SelectListItem"/> is selected.
		/// This property is typically rendered as a <code>selected="selected"</code> attribute in the HTML
		/// <code>&lt;option&gt;</code> element.
		/// </summary>
		public bool Selected { get; set; }

		/// <summary>
		/// Gets or sets a value that indicates the display text of this <see cref="SelectListItem"/>.
		/// This property is typically rendered as the inner HTML in the HTML <code>&lt;option&gt;</code> element.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Gets or sets a value that indicates the value of this <see cref="SelectListItem"/>.
		/// This property is typically rendered as a <code>value="..."</code> attribute in the HTML
		/// <code>&lt;option&gt;</code> element.
		/// </summary>
		public string Value { get; set; }
	}

	///// <summary>
	///// Represents the optgroup HTML element and its attributes.
	///// In a select list, multiple groups with the same name are supported.
	///// They are compared with reference equality.
	///// </summary>
	//public class SelectListGroup
	//{
	//	/// <summary>
	//	/// Gets or sets a value that indicates whether this <see cref="SelectListGroup"/> is disabled.
	//	/// </summary>
	//	public bool Disabled { get; set; }

	//	/// <summary>
	//	/// Represents the value of the optgroup's label.
	//	/// </summary>
	//	public string Name { get; set; }
	//}
}
