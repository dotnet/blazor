using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Microsoft.AspNetCore.Blazor.Forms.Internals
{
	internal class CustomPropertiesProvider
	{
		internal static PropertyDescriptorCollection GetPropertiesInternal( Type type, Attribute[] attributes, MasqueradeObjectBase parent )
		{
			Console.WriteLine("GetPropertiesInternal");

			List<MasqueradeProperty> list = new List<MasqueradeProperty>();
			var properties = TypeDescriptor.GetProperties(type, attributes);
			foreach (PropertyDescriptor prop in properties)
			{
				Console.WriteLine($"prop={prop.Name}");

				var mp = new MasqueradeProperty(prop, parent);
				if (prop.PropertyType == typeof(int))
					mp.CustomAttributes.Add(new IntegerValidationAttribute());
				else if (prop.PropertyType == typeof(double))
					mp.CustomAttributes.Add(new DoubleValidationAttribute());
				else if (prop.PropertyType == typeof(float))
					mp.CustomAttributes.Add(new FloatValidationAttribute());
				else if (prop.PropertyType == typeof(decimal))
					mp.CustomAttributes.Add(new DecimalValidationAttribute());
				else if (prop.PropertyType == typeof(bool))
					mp.CustomAttributes.Add(new BooleanValidationAttribute());

				list.Add(mp);
			}
			return new PropertyDescriptorCollection(list.ToArray());
		}
	}
}
