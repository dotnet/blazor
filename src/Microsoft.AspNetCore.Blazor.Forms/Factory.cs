using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Blazor.Forms
{
	/// <summary>
	/// 
	/// </summary>
	public static class Factory
	{
		/// <summary>
		/// 
		/// </summary>
		public static int DropZoneComponentId { get; private set; }

		/// <summary>
		/// Register custom DOMComponent
		/// </summary>
		public static void Register()
		{
            //DropZoneComponentId = Microsoft.AspNetCore.Blazor.Components.ComponentFactory.RegisterCustomComponent(typeof(Components.DropZone));
            try
            {
				((IJSInProcessRuntime)JSRuntime.Current).Invoke<bool>("RegisterDropZoneComponentId", DropZoneComponentId);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }
		}
	}
}
