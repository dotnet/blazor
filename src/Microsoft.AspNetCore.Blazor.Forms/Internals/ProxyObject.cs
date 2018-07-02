using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Blazor.Forms.Internals
{
	/// <summary>
	/// </summary>
	internal class MasqueradeObjectTypeDescriptionProvider : TypeDescriptionProvider
	{
		private static TypeDescriptionProvider defaultTypeProvider =
								TypeDescriptor.GetProvider(typeof(ProxyObject<>));

		/// <summary>
		/// </summary>
		public MasqueradeObjectTypeDescriptionProvider() : base(defaultTypeProvider)
		{
		}

		/// <summary>
		/// </summary>
		public override ICustomTypeDescriptor GetTypeDescriptor( Type objectType, object instance )
		{
			ICustomTypeDescriptor defaultDescriptor = base.GetTypeDescriptor(objectType, instance);
			return new MasqueradeObjectTypeDescription(defaultDescriptor, objectType, instance);
		}
	}

	class MasqueradeObjectTypeDescription : CustomTypeDescriptor
	{
		Type _parentType;
		object _instance;

		public MasqueradeObjectTypeDescription( ICustomTypeDescriptor parent, Type objectType, object instance ) : base(parent)
		{
			_parentType = objectType.GetGenericArguments()[0];
			_instance = instance;
		}

		private List<PropertyDescriptor> customFields = new List<PropertyDescriptor>();

		public override PropertyDescriptorCollection GetProperties()
		{
			return GetProperties(null);
		}

		public override AttributeCollection GetAttributes()
		{
			var list = base.GetAttributes();
			return list;
		}

		public override PropertyDescriptorCollection GetProperties( Attribute[] attributes )
		{
			return CustomPropertiesProvider.GetPropertiesInternal(_parentType, attributes, _instance as ProxyObjectBase);
		}
	}

	/// <summary>
	/// </summary>
	internal class ProxyObjectBase
    {
		/// <summary>
		/// </summary>
		protected readonly object _parent;

		/// <summary>
		/// </summary>
		public ProxyObjectBase( object parent )
		{
			_parent = parent;
		}

		/// <summary>
		/// </summary>
		public Func<PropertyDescriptor, object> GetValue { get; set; }

		internal object _GetValue( PropertyDescriptor pd, object component )
		{
			if (GetValue != null)
				return GetValue(pd);
			else
				return pd.GetValue(component);
		}
       
        internal bool TryValidateObject(ValidationContext validationContext, List<ValidationResult> validationResults, System.Collections.Generic.List<string> PropertiesToValidate)
        {
            var properties = TypeDescriptor.GetProperties(this);

            bool isValid = true;
            foreach (PropertyDescriptor prop in properties)
            {
                if (PropertiesToValidate != null && !PropertiesToValidate.Contains(prop.Name))
                    continue;

                object value = this.GetValue(prop);
                var attrs = prop.Attributes.OfType<ValidationAttribute>();

                validationContext.MemberName = prop.Name;
                validationContext.DisplayName = prop.DisplayName;

                bool _valid = Validator.TryValidateValue(value, validationContext, validationResults, attrs);
                isValid &= _valid;

                //Console.WriteLine($"Validate: {prop.Name} value: {value} attrs:{attrs?.Count()} valid={_valid}");
            }

            //var custom = this._parent as IValidatableObject;
            //if (custom != null)
            //    custom.Validate(validationContext);

            return isValid;
        }
    }

	/// <summary>
	/// </summary>
	[TypeDescriptionProvider(typeof(MasqueradeObjectTypeDescriptionProvider))]
	internal class ProxyObject<T> : ProxyObjectBase, System.ComponentModel.ICustomTypeDescriptor
	{
		Type _parentType;

		/// <summary>
		/// </summary>
		public ProxyObject( T parent ) : base(parent)
		{
			_parentType = parent.GetType();
		}

		/// <summary>
		/// </summary>
		public AttributeCollection GetAttributes()
		{
			return TypeDescriptor.GetAttributes(_parentType);
		}

		/// <summary>
		/// </summary>
		public string GetClassName()
		{
			return TypeDescriptor.GetClassName(_parentType);
		}

		/// <summary>
		/// </summary>
		public string GetComponentName()
		{
			return TypeDescriptor.GetComponentName(_parentType);
		}

		/// <summary>
		/// </summary>
		public TypeConverter GetConverter()
		{
			return TypeDescriptor.GetConverter(_parentType);
		}

		/// <summary>
		/// </summary>
		public EventDescriptor GetDefaultEvent()
		{
			return TypeDescriptor.GetDefaultEvent(_parentType);
		}

		/// <summary>
		/// </summary>
		public PropertyDescriptor GetDefaultProperty()
		{
			return TypeDescriptor.GetDefaultProperty(_parentType);
		}

		/// <summary>
		/// </summary>
		public object GetEditor( Type editorBaseType )
		{
			return null; // TypeDescriptor.GetEditor(_parentType);
		}

		/// <summary>
		/// </summary>
		public EventDescriptorCollection GetEvents()
		{
			return TypeDescriptor.GetEvents(_parentType);
		}

		/// <summary>
		/// </summary>
		public EventDescriptorCollection GetEvents( Attribute[] attributes )
		{
			return TypeDescriptor.GetEvents(_parentType, attributes);
		}

		/// <summary>
		/// </summary>
		public PropertyDescriptorCollection GetProperties()
		{
			return GetProperties(null);
		}

		/// <summary>
		/// </summary>
		public PropertyDescriptorCollection GetProperties( Attribute[] attributes )
		{
			var list = CustomPropertiesProvider.GetPropertiesInternal(_parentType, attributes, this);
			return list;
		}

		/// <summary>
		/// </summary>
		public object GetPropertyOwner( PropertyDescriptor pd )
		{
			return this._parent;
		}
    }

	internal class MasqueradeProperty : PropertyDescriptor
	{
		PropertyDescriptor _pd;
		ProxyObjectBase _parent;

		internal List<Attribute> CustomAttributes { get; } = new List<Attribute>();

		internal MasqueradeProperty( PropertyDescriptor md, ProxyObjectBase parent ) : base(md)
		{
			_pd = md;
			_parent = parent;
		}

		public override bool ShouldSerializeValue( object component )
		{
			return _pd.ShouldSerializeValue(component);
		}

		public override Type PropertyType => _pd.PropertyType;
		public override Type ComponentType => _pd.ComponentType;
		public override bool IsReadOnly => _pd.IsReadOnly;

		public override AttributeCollection Attributes
		{
			get
			{
				var coll = new AttributeCollection(base.Attributes.Cast<Attribute>().Union(CustomAttributes).ToArray());
				return coll;
			}
		}

		public override void SetValue( object component, object value )
		{
			throw new NotSupportedException();
		}

		public override object GetValue( object component )
		{
			return _parent._GetValue(_pd, component);
		}

		public override bool CanResetValue( object component )
		{
			return _pd.CanResetValue(component);
		}

		public override void ResetValue( object component )
		{
			_pd.ResetValue(component);
		}
	}

    internal class ValidationAttributePropertyValidator
    {
        protected virtual IEnumerable<ValidationAttribute> GetAttributes(MemberInfo propertyInfo) {
            return propertyInfo.GetCustomAttributes(typeof(ValidationAttribute), true).Cast<ValidationAttribute>();
        }
    }
}
