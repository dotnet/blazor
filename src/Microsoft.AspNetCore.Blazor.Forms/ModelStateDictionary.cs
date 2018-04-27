using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Microsoft.AspNetCore.Blazor.Forms
{
	/// <summary>
	/// 
	/// </summary>
	public class ModelStateDictionary : Dictionary<string, object>
	{
		object _binder;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="binder"></param>
		public ModelStateDictionary( object binder )
		{
			_binder = binder;
		}

		internal object GetValue( PropertyDescriptor property )
		{
			if (this.ContainsKey(property.Name))
				return this[property.Name];
			else
				return property.GetValue(_binder);
		}

		internal string GetValue( PropertyInfo property )
		{
			if (this.ContainsKey(property.Name))
				return this[property.Name]?.ToString();
			else
				return property.GetValue(_binder)?.ToString();
		}

		internal void SetValue( PropertyInfo property, object parsedValue )
		{
			var propertyType = property.PropertyType;
			this[property.Name] = parsedValue;

			//if (propertyType == typeof(string))
			//	property.SetValue(_binder, (string)parsedValue);
			//else if (propertyType == typeof(int))
			//{
			//	int v = 0;
			//	if (int.TryParse(parsedValue, out v))
			//		property.SetValue(_binder, v);
			//}
		}
	}
}
