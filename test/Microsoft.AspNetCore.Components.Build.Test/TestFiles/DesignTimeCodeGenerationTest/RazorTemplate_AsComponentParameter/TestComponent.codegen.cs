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
    public class TestComponent : Microsoft.AspNetCore.Components.ComponentBase
    {
        #pragma warning disable 219
        private void __RazorDirectiveTokenHelpers__() {
        ((System.Action)(() => {
global::System.Object __typeHelper = "*, TestAssembly";
        }
        ))();
        }
        #pragma warning restore 219
        #pragma warning disable 0414
        private static System.Object __o = null;
        #pragma warning restore 0414
        #pragma warning disable 1998
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.RenderTree.RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
#line 2 "x:\dir\subdir\Test\TestComponent.cshtml"
   RenderFragment<Person> template = 

#line default
#line hidden
            (builder2) => {
#line 2 "x:\dir\subdir\Test\TestComponent.cshtml"
                                      __o = context.Name;

#line default
#line hidden
            }
#line 2 "x:\dir\subdir\Test\TestComponent.cshtml"
                                                              ; 

#line default
#line hidden
            __o = new Microsoft.AspNetCore.Blazor.RenderFragment<Test.Person>(
#line 3 "x:\dir\subdir\Test\TestComponent.cshtml"
                              template

#line default
#line hidden
            );
            builder.AddAttribute(-1, "ChildContent", (Microsoft.AspNetCore.Blazor.RenderFragment)((builder2) => {
            }
            ));
        }
        #pragma warning restore 1998
    }
}
#pragma warning restore 1591
