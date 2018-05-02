import { BrowserRenderer } from '../BrowserRenderer';
import { BlazorDOMElement, getBlazorDomElement } from './BlazorDOMElement';
import { BlazorDOMComponent } from './BlazorDOMComponent';
import { BlazorINPUTElement } from './BlazorINPUTElement';

import { getRegisteredCustomTag, getRegisteredCustomDOMElement } from '../../Interop/RenderingFunction';

export function createBlazorDOMElement(br: BrowserRenderer, stepInElement: Element): BlazorDOMElement {
	let element = getBlazorDomElement(stepInElement);
	if (element !== undefined) return element;

	if (stepInElement.tagName == "INPUT" || stepInElement.tagName == "SELECT" || stepInElement.tagName == "TEXTAREA")
		return new BlazorINPUTElement(br, stepInElement);
	else {
		let customDOM = getRegisteredCustomTag(stepInElement.tagName) as any;
		if (customDOM !== null) {
			return customDOM(br, stepInElement);
		}
		else {
			return new BlazorDOMElement(br, stepInElement);
		}
	}
}

export function createBlazorDOMComponent(br: BrowserRenderer, componentId: number, parent: BlazorDOMElement, childIndex: number, customComponentType: number): BlazorDOMComponent {
	let blazorElement: BlazorDOMComponent | null = null;

	if (customComponentType !== 0) {
		let customElement = getRegisteredCustomDOMElement(customComponentType) as any;
		blazorElement = customElement(componentId, parent, childIndex, br);
	}
	else {
		blazorElement = new BlazorDOMComponent(componentId, parent, childIndex, br);
	}
	return blazorElement!;
}