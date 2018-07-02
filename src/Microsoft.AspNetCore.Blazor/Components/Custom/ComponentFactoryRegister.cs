using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Blazor.Components
{
	/// <summary>
	/// </summary>
	public static class ComponentFactoryRegister
  {
		private static List<Type> _RegisteredCustomDOMElement = new List<Type>();

		/// <summary>
		/// Register a javascript implementation of Blazor Component.
		/// </summary>
		/// <param name="ComponentType">Type of Blazor Component</param>
		/// <returns>
		/// Returns a value that bind javascript implementation with .net component. 
		/// (That value is used in BrowserRenderer's getRegisteredCustomDOMElement)
		/// </returns>
		public static short RegisterCustomComponent(Type ComponentType)
		{
			// need check for ComponentType
			if (_RegisteredCustomDOMElement.Contains(ComponentType) == false)
				_RegisteredCustomDOMElement.Add(ComponentType);

			var componentId = (short)(_RegisteredCustomDOMElement.IndexOf(ComponentType) + 1);
			return componentId;
		}

		/// <summary>
		/// Get the registrered custom BlazorDOMComponent
		/// </summary>
		/// <param name="ComponentType">BlazorComponent Type</param>
		/// <returns>
		/// A short for CustomComponentType for RenderTreeFrameType.Component
		/// </returns>
		public static short GetRegisteredCustomComponent(Type ComponentType)
		{
			if (_RegisteredCustomDOMElement.Contains(ComponentType) == false)
				return 0;

			return (short)(_RegisteredCustomDOMElement.IndexOf(ComponentType) + 1);
		}
	}
}
