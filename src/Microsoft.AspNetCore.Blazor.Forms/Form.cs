using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace Microsoft.AspNetCore.Blazor.Forms
{
	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Form<T> : Microsoft.AspNetCore.Blazor.Components.BlazorComponent, IForm<T>
	{
        /// <summary>
        /// </summary>
        public ModelStateDictionary<T> ModelState { get; set; }

		private T _Model;

        /// <summary>
        /// </summary>
        [Parameter]
        internal protected T Model
		{
			get { return _Model; }
			set
			{
				_Model = value;
				ModelState = CreateModelState(value);
			}
		}

        /// <summary>
        /// </summary>
        protected virtual ModelStateDictionary<T> CreateModelState(T value)
        {
            return new ModelStateDictionary<T>(value);
        }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// Invoke ValidateModel()
        /// </summary>
        protected override void OnParametersSet()
		{
			base.OnParametersSet();
			ModelState?.ValidateModel();
		}

        /// <summary>
        /// </summary>
        public void ValidateModel()
        {
            bool currentIsValid = ModelState.IsValid;

            ModelState?.ValidateModel();
            if (currentIsValid != ModelState.IsValid) this.StateHasChanged();
        }
    }
}
