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
#line 2 "x:\dir\subdir\Test\TestComponent.cshtml"
   RenderFragment<Test.Context> template = (context) => 

#line default
#line hidden
            (builder2) => {
                builder2.OpenElement(0, "li");
                builder2.AddContent(1, "#");
                builder2.AddContent(2, context.Index);
                builder2.AddContent(3, " - ");
                builder2.AddContent(4, context.Item.ToLower());
                builder2.CloseElement();
            }
#line 2 "x:\dir\subdir\Test\TestComponent.cshtml"
                                                                                                           ; 

#line default
#line hidden
            builder.OpenComponent<Test.MyComponent>(5);
            builder.AddAttribute(6, "Template", new Microsoft.AspNetCore.Components.RenderFragment<Test.Context>(template));
            builder.CloseComponent();
        }
        #pragma warning restore 1998
    }
}
#pragma warning restore 1591
