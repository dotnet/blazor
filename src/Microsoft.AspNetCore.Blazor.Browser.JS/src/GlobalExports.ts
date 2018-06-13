import { platform } from './Environment'
import { registerFunction } from './Interop/RegisteredFunction';
import { BlazorDOMComponent } from './Rendering/Elements/BlazorDOMComponent'
import { BlazorDOMElement } from './Rendering/Elements/BlazorDOMElement'
import { registerCustomTag, registerCustomDOMElement } from './Interop/RenderingFunction'
import { raiseEvent } from './Rendering/BrowserRenderer'
import { navigateTo } from './Services/UriHelper';
import { invokeDotNetMethod, invokeDotNetMethodAsync } from './Interop/InvokeDotNetMethodWithJsonMarshalling';

if (typeof window !== 'undefined') {
  // When the library is loaded in a browser via a <script> element, make the
  // following APIs available in global scope for invocation from JS
  window['Blazor'] = {
    platform,
    registerFunction,
    navigateTo,
    invokeDotNetMethod,
    invokeDotNetMethodAsync

    ,
    raiseEvent,
		registerCustomTag,
		registerCustomDOMElement,
		BlazorDOMElement,
		BlazorDOMComponent
  };
}
