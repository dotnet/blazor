import { BrowserRenderer } from '../BrowserRenderer';
import { renderTreeFrame, RenderTreeFramePointer } from '../RenderTreeFrame';
import { getRegisteredCustomTag } from '../../Interop/RenderingFunction';
import { BlazorINPUTElement } from './BlazorINPUTElement';

const logicalBlazorDomElementPropname = createSymbolOrFallback('_blazorDomElement');
//const logicalBlazorDomChildArrayPropname = createSymbolOrFallback('_blazorDomChildArrayPropname');

export class BlazorDOMElement {
	protected readonly browserRenderer: BrowserRenderer;
	private _elements: Node[] = [];

	private startContainer: Node;
	private endContainer: Node | null;

	constructor(browserRendeder: BrowserRenderer, start: Node, end: Node | null = null) {
		this.browserRenderer = browserRendeder;

		this.startContainer = start;
		this.endContainer = end;

		this.startContainer[logicalBlazorDomElementPropname] = this;
	}

	protected isComponent(): boolean {
		return false;
	}

	public getClosestDomElement(): Node {
		return this.startContainer;
	}

	protected getDOMElement(): HTMLElement {
		return this.startContainer as HTMLElement;
	}

	public getLogicalChild(childIndex: number): Node | BlazorDOMElement | null {
		let element: Node | null = this.startContainer;
		if (this.isComponent() === false)
			element = element.firstChild;
		else
			element = element.nextSibling;

		if (element == null) {
			// no child
			return null;
		}
		else {
			while (childIndex > 0) {
				// skip if is this.Range
				if (element !== this.startContainer) {
					// is a component ?
					let blazorDom = this.getComponentFromNode(element);
					if (blazorDom != null) {
						element = blazorDom.endContainer;
					}
				}

				childIndex--;

				element = element!.nextSibling;
				if (element == null) return null;
			}

			let blazorDom = this.getComponentFromNode(element);
			if (blazorDom != null) {
				return blazorDom;
			}

			return element;
		}
	}

	private getComponentFromNode(element: Node): BlazorDOMElement | null {
		let component = element[logicalBlazorDomElementPropname] as BlazorDOMElement;
		if (component !== null && component !== undefined && component.isComponent() === true) return component;
		return null;
	}

	public insertNodeIntoDOM(node: Node, childIndex: number) {
		let parentElement = this.getClosestDomElement();

		let realSibling = this.getLogicalChild(childIndex);
		if (realSibling === null) {
			if (this.isComponent() == false) {
				parentElement.appendChild(node);
			}
			else {
				parentElement.insertBefore(node, this.endContainer);
			}
		}
		else {
			parentElement.insertBefore(node, realSibling as Node);
		}
	}

	public removeFromDom(childIndex: number | null = null) {
		if (childIndex === null) {
			// Adjust range to whole component
			var range = document.createRange();
			range.setStartBefore(this.startContainer);
			range.setEndAfter(this.endContainer!);

			// Clear whole range
			range.deleteContents();
			range.detach();
		}
		else {
			const element = this.getLogicalChild(childIndex)!;

			if (element instanceof BlazorDOMElement) {
				element.removeFromDom();
			}
			else {
				// Remove only the childindex-nth element
				element.parentElement!.removeChild(element as Node);
			}
		}
	}

	public updateText(childIndex: number, newText: string | null) {
		const domTextNode = this.getLogicalChild(childIndex) as Text;
		domTextNode.textContent = newText;
	}

	public applyAttribute(componentId: number, attributeFrame: RenderTreeFramePointer) {
		//const toDomElement = this.Range.startContainer as Element;
		//const browserRendererId = this.browserRenderer.browserRendererId;
		const attributeName = renderTreeFrame.attributeName(attributeFrame)!;
		const attributeValue = renderTreeFrame.attributeValue(attributeFrame);

		if (this.isDOMAttribute(attributeName, attributeValue) == false) {
			return; // If this DOM element type has special 'value' handling, don't also write it as an attribute
		}

		const eventHandlerId = renderTreeFrame.attributeEventHandlerId(attributeFrame);
		if (this.applyEvent(attributeName, componentId, eventHandlerId) == false) {
			// Treat as a regular string-valued attribute
			this.setAttribute(attributeName, attributeValue);
		}
	}

	public removeAttribute(childIndex: number, attributeName: string) {
		// maybe must be rewritten (never go inside for now)
		const element = this.getLogicalChild(childIndex) as Element;
		element.removeAttribute(attributeName);
	}

	protected setAttribute(attributeName: string, attributeValue: string | null) {
		const toDomElement = this.startContainer as Element;

		if (attributeValue == null) {
			toDomElement.removeAttribute(attributeName);
		}
		else {
			toDomElement.setAttribute(attributeName, attributeValue);
		}
	}

	protected isDOMAttribute(attributeName: string, value: string | null): boolean {
		// default is true
		return true;
	}

	protected applyEvent(attributeName: string, componentId: number, eventHandlerId: number): boolean {
		const toDomElement = this.startContainer as Element;
		//const browserRendererId = this.browserRenderer.browserRendererId;

		if (eventHandlerId) {
			const firstTwoChars = attributeName.substring(0, 2);
			const eventName = attributeName.substring(2);
			if (firstTwoChars !== 'on' || !eventName) {
				throw new Error(`Attribute has nonzero event handler ID, but attribute name '${attributeName}' does not start with 'on'.`);
			}
			this.browserRenderer.eventDelegator.setListener(toDomElement, eventName, componentId, eventHandlerId);
			return true;
		}

		return false;
	}

	public onDOMUpdating() { }
	public onDOMUpdated() { }

	public dispose() {
	}
}

export function getBlazorDomElement(container: Node) {
	return container[logicalBlazorDomElementPropname];
}

function createSymbolOrFallback(fallback: string): symbol | string {
    return typeof Symbol === 'function' ? Symbol() : fallback;
}