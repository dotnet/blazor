using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microsoft.AspNetCore.Blazor.Forms.Internals
{
    internal class NullableValidationAttributeBase : ValidationAttribute
    {
        bool _PermitStringEmpty;

        internal NullableValidationAttributeBase(bool PermitStringEmpty)
        {
            _PermitStringEmpty = PermitStringEmpty;
        }

        public override bool IsValid(object value)
        {
            var vString = value?.ToString();
            if (_PermitStringEmpty && string.IsNullOrWhiteSpace(vString))
                return true;
            else
            {
                return CheckValid(vString);
            }
        }

        protected virtual bool CheckValid(string vString)
        {
            return false;
        }
    }

    internal class IntegerValidationAttribute : NullableValidationAttributeBase
    {
        internal IntegerValidationAttribute(bool PermitStringEmpty) : base(PermitStringEmpty)
		{
		}

		protected override bool CheckValid( string vString)
		{
    		if (int.TryParse(vString, out int v))
				return true;
			else
				return false;
		}
	}

	internal class DoubleValidationAttribute : NullableValidationAttributeBase
    {
		internal DoubleValidationAttribute(bool PermitStringEmpty) : base(PermitStringEmpty)
        {
		}

        protected override bool CheckValid(string vString)
        {
            if (double.TryParse(vString, out double v))
				return true;
			else
				return false;
		}
	}

	internal class FloatValidationAttribute : NullableValidationAttributeBase
    {
		internal FloatValidationAttribute(bool PermitStringEmpty) : base(PermitStringEmpty)
        {
		}

        protected override bool CheckValid(string vString)
        {
            if (float.TryParse(vString, out float v))
				return true;
			else
				return false;
		}
	}

	internal class DecimalValidationAttribute : NullableValidationAttributeBase
    {
		internal DecimalValidationAttribute(bool PermitStringEmpty) : base(PermitStringEmpty)
        {
		}

        protected override bool CheckValid(string vString)
        {
            if (decimal.TryParse(vString, out decimal v))
				return true;
			else
				return false;
		}
	}

	internal class BooleanValidationAttribute : NullableValidationAttributeBase
    {
		internal BooleanValidationAttribute(bool PermitStringEmpty) : base(PermitStringEmpty)
        {
		}

        protected override bool CheckValid(string vString)
        {
            if (bool.TryParse(vString, out bool v))
				return true;
			else
				return false;
		}
	}

    internal class DateTimeValidationAttribute : NullableValidationAttributeBase
    {
        internal DateTimeValidationAttribute(bool PermitStringEmpty) : base(PermitStringEmpty)
        {
        }

        protected override bool CheckValid(string vString)
        {
            if (System.DateTime.TryParse(vString, out System.DateTime v))
                return true;
            else
                return false;
        }
    }
}
