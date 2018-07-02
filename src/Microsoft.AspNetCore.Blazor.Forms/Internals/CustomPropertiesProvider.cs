using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Microsoft.AspNetCore.Blazor.Forms.Internals
{
	internal class CustomPropertiesProvider
	{
		internal static PropertyDescriptorCollection GetPropertiesInternal( Type type, Attribute[] attributes, ProxyObjectBase parent )
		{
			//Console.WriteLine("GetPropertiesInternal");

			List<MasqueradeProperty> list = new List<MasqueradeProperty>();
			var properties = TypeDescriptor.GetProperties(type, attributes);
			foreach (PropertyDescriptor prop in properties)
			{
                //Console.WriteLine($"prop={prop.Name}");

                var propertyType = prop.PropertyType;
                var isNullable = false;
                var nullableType = Nullable.GetUnderlyingType(propertyType);
                if( nullableType != null )
                {
                    propertyType = nullableType;
                    isNullable = true;
                }

                var mp = new MasqueradeProperty(prop, parent);
				if (propertyType == typeof(int))
					mp.CustomAttributes.Add(new IntegerValidationAttribute(isNullable));
                else if (propertyType == typeof(double))
					mp.CustomAttributes.Add(new DoubleValidationAttribute(isNullable));
				else if (propertyType == typeof(float))
					mp.CustomAttributes.Add(new FloatValidationAttribute(isNullable));
				else if (propertyType == typeof(decimal))
					mp.CustomAttributes.Add(new DecimalValidationAttribute(isNullable));
				else if (propertyType == typeof(bool))
					mp.CustomAttributes.Add(new BooleanValidationAttribute(isNullable));
                else if (propertyType == typeof(System.DateTime))
                    mp.CustomAttributes.Add(new DateTimeValidationAttribute(isNullable));

                list.Add(mp);
			}
			return new PropertyDescriptorCollection(list.ToArray());
		}
	}
}
