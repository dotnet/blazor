// <auto-generated/>
#pragma warning disable 1591
namespace Test
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
#line 1 "x:\dir\subdir\Test\TestComponent.cshtml"
using System.Threading.Tasks;

#line default
#line hidden
    using Microsoft.AspNetCore.Components;
    public class TestComponent : Microsoft.AspNetCore.Components.BlazorComponent
    {
        #pragma warning disable 219
        private void __RazorDirectiveTokenHelpers__() {
        }
        #pragma warning restore 219
        #pragma warning disable 0414
        private static System.Object __o = null;
        #pragma warning restore 0414
        #pragma warning disable 1998
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.RenderTree.RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            __o = Microsoft.AspNetCore.Components.BindMethods.GetEventHandlerValue<Microsoft.AspNetCore.Components.UIMouseEventArgs>(OnClick);
        }
        #pragma warning restore 1998
#line 3 "x:\dir\subdir\Test\TestComponent.cshtml"
            
    Task OnClick() 
    {
        return Task.CompletedTask;
    }

#line default
#line hidden
    }
}
#pragma warning restore 1591
