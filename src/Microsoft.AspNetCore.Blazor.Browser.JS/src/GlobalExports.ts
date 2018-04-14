import { platform } from './Environment'
import { registerFunction } from './Interop/RegisteredFunction';
import { BlazorDOMElement, BlazorDOMComponent, raiseEvent } from './Rendering/BrowserRenderer'
import { registerCustomTag, registerCustomDOMElement } from './Interop/RenderingFunction'
import { navigateTo } from './Services/UriHelper';

if (typeof window !== 'undefined') {
  // When the library is loaded in a browser via a <script> element, make the
  // following APIs available in global scope for invocation from JS
  window['Blazor'] = {
    platform,
    registerFunction,
		navigateTo,

		raiseEvent,
		registerCustomTag,
		registerCustomDOMElement,
		BlazorDOMElement,
		BlazorDOMComponent
  };
}
