using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microsoft.AspNetCore.Blazor.Forms.Internals
{
	internal class IntegerValidationAttribute : ValidationAttribute
	{
		internal IntegerValidationAttribute()
		{
		}

		public override bool IsValid( object value )
		{
			if (int.TryParse(value?.ToString(), out int v))
				return true;
			else
				return false;
		}
	}

	internal class DoubleValidationAttribute : ValidationAttribute
	{
		internal DoubleValidationAttribute()
		{
		}

		public override bool IsValid( object value )
		{
			if (double.TryParse(value?.ToString(), out double v))
				return true;
			else
				return false;
		}
	}

	internal class FloatValidationAttribute : ValidationAttribute
	{
		internal FloatValidationAttribute()
		{
		}

		public override bool IsValid( object value )
		{
			if (float.TryParse(value?.ToString(), out float v))
				return true;
			else
				return false;
		}
	}

	internal class DecimalValidationAttribute : ValidationAttribute
	{
		internal DecimalValidationAttribute()
		{
		}

		public override bool IsValid( object value )
		{
			if (decimal.TryParse(value?.ToString(), out decimal v))
				return true;
			else
				return false;
		}
	}

	internal class BooleanValidationAttribute : ValidationAttribute
	{
		internal BooleanValidationAttribute()
		{
		}

		public override bool IsValid( object value )
		{
			if (bool.TryParse(value?.ToString(), out bool v))
				return true;
			else
				return false;
		}
	}
}
