using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace Microsoft.AspNetCore.Blazor.Forms.Extensions
{
    /// <summary>
    /// </summary>
	public static class PropertyHelpers
	{
        /// <summary>
        /// </summary>
		public static PropertyInfo GetProperty<T,V>( Expression<Func<T, V>> field )
		{
			LambdaExpression lambda = field as LambdaExpression;
			MemberExpression memberExpr = null;
			if (lambda.Body.NodeType == ExpressionType.Convert)
			{
				memberExpr = ((UnaryExpression)lambda.Body).Operand as MemberExpression;
			}
			else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
			{
				memberExpr = lambda.Body as MemberExpression;
			}
			if (memberExpr == null) throw new ArgumentException("method");

			var property = memberExpr.Member as System.Reflection.PropertyInfo;
			return property;
		}
	}
}
