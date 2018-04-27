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
	public class MasqueradeObjectTypeDescriptionProvider : TypeDescriptionProvider
	{
		private static TypeDescriptionProvider defaultTypeProvider =
								TypeDescriptor.GetProvider(typeof(MasqueradeObject<>));

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
			return CustomPropertiesProvider.GetPropertiesInternal(_parentType, attributes, _instance as MasqueradeObjectBase);
		}
	}

	/// <summary>
	/// </summary>
	public class MasqueradeObjectBase /*: System.Dynamic.DynamicObject*/
    {
		/// <summary>
		/// </summary>
		protected readonly object _parent;

		/// <summary>
		/// </summary>
		public MasqueradeObjectBase( object parent )
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

        //public IEnumerable<ValidationResult> Validate( ValidationContext validationContext )
        //{
        //	List<ValidationResult> result = new List<ValidationResult>();

        //	var properties = TypeDescriptor.GetProperties(_parent.GetType());
        //	foreach (PropertyDescriptor prop in properties)
        //	{
        //		if (prop.PropertyType == typeof(int))
        //		{
        //			var obj = GetValue(prop)?.ToString();
        //			int check = 0;
        //			if (int.TryParse(obj, out check) == false)
        //			{
        //				string errorMessage = $"Il formato di {validationContext.DisplayName} non è valido";
        //				result.Add(new ValidationResult(errorMessage, new string[] { validationContext.MemberName }));
        //			}
        //		}
        //	}

        //	return result;
        //}
        
        internal bool TryValidateObject(ValidationContext validationContext, List<ValidationResult> validationResults, bool allValues)
        {
            var properties = TypeDescriptor.GetProperties(this);

            bool isValid = true;
            foreach (PropertyDescriptor prop in properties)
            {
                object value = this.GetValue(prop);
                var attrs = prop.Attributes.OfType<ValidationAttribute>();

                validationContext.MemberName = prop.Name;
                validationContext.DisplayName = prop.DisplayName;
                isValid &= Validator.TryValidateValue(value, validationContext, validationResults, attrs);
            }

            return isValid;
        }
    }

	/// <summary>
	/// </summary>
	[TypeDescriptionProvider(typeof(MasqueradeObjectTypeDescriptionProvider))]
	public class MasqueradeObject<T> : MasqueradeObjectBase, System.ComponentModel.ICustomTypeDescriptor
	{
		Type _parentType;

		/// <summary>
		/// </summary>
		public MasqueradeObject( T parent ) : base(parent)
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
		MasqueradeObjectBase _parent;

		internal List<Attribute> CustomAttributes { get; } = new List<Attribute>();

		internal MasqueradeProperty( PropertyDescriptor md, MasqueradeObjectBase parent ) : base(md)
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

    //    #region IPropertyValidator interface

    //    public virtual IEnumerable<string> GetValidationProperties(object proxiedObject)
    //    {
    //        if (proxiedObject == null)
    //            throw new ArgumentNullException("proxy");
    //        return proxiedObject.GetType().GetProperties().Where(pi => GetAttributes(pi).Any()).Select(pi => pi.Name);
    //    }

    //    public bool Validate(object proxiedObject, string propertyName, object value, ICollection<ValidationResult> validationResults)
    //    {
    //        var info = GlobalTypeInfoCache.GetTypeInfo(proxiedObject.GetType()).GetPropertyInfo(propertyName);
    //        var validationAttributes = GetAttributes(info);
    //        if (validationAttributes.Count() == 0)
    //            return true;

    //        var validationContext = new ValidationContext(proxiedObject, null, null);

    //        var isValid = Validator.TryValidateValue(value, validationContext, validationResults, validationAttributes);

    //        if (isValid)
    //        {
    //            var propertyType = proxiedObject.GetPropertyType(propertyName);
    //            try
    //            {
    //                if (propertyType != value.GetType())
    //                    Convert.ChangeType(value, propertyType);
    //            }
    //            catch (Exception)
    //            {
    //                validationResults.Add(new ValidationResult("Cannot convert value to type " + propertyType));
    //                isValid = false;
    //            }
    //        }
    //        return isValid;
    //    }

    //    #endregion
    }
}
