// <auto-generated/>
#pragma warning disable 1591
namespace Test
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    public class TestComponent : Microsoft.AspNetCore.Components.BlazorComponent
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.RenderTree.RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            builder.OpenComponent<Test.DynamicElement>(0);
            builder.AddAttribute(1, "onclick", Microsoft.AspNetCore.Components.BindMethods.GetEventHandlerValue<Microsoft.AspNetCore.Components.UIMouseEventArgs>(OnClick));
            builder.CloseComponent();
        }
        #pragma warning restore 1998
#line 4 "x:\dir\subdir\Test\TestComponent.cshtml"
            
    private Action<UIMouseEventArgs> OnClick { get; set; }

#line default
#line hidden
    }
}
#pragma warning restore 1591
