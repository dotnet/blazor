// <auto-generated/>
#pragma warning disable 1591
namespace Test
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Blazor;
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
#line 1 "x:\dir\subdir\Test\TestComponent.cshtml"
__o = RenderPerson((builder2) => {
#line 1 "x:\dir\subdir\Test\TestComponent.cshtml"
               __o = context.Name;

#line default
#line hidden
}
);

#line default
#line hidden
        }
        #pragma warning restore 1998
#line 2 "x:\dir\subdir\Test\TestComponent.cshtml"
            
    class Person
    {
        public string Name { get; set; }
    }

    object RenderPerson(RenderFragment<Person> p) => null;

#line default
#line hidden
    }
}
#pragma warning restore 1591
