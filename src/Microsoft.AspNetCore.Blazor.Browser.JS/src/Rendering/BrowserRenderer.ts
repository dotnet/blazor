import { System_Array, MethodHandle } from '../Platform/Platform';
import { getRenderTreeEditPtr, renderTreeEdit, RenderTreeEditPointer, EditType } from './RenderTreeEdit';
import { getTreeFramePtr, renderTreeFrame, FrameType, RenderTreeFramePointer } from './RenderTreeFrame';
import { platform } from '../Environment';
let raiseEventMethod: MethodHandle;
let renderComponentMethod: MethodHandle;

class BlazorDOMElement {
	private Range: Range;
	protected readonly browserRenderer: BrowserRenderer;

	constructor(browserRendeder: BrowserRenderer, start: Node, end: Node | null = null) {
		this.browserRenderer = browserRendeder;
		this.Range = document.createRange();
		this.Range.setStart(start, 0);
		if (end !== null) this.Range.setEnd(end, 0);
	}

	public isComponent(): boolean {
		return !this.Range.collapsed;
	}

	public getParentDOMElement(): Node {
		return this.Range.commonAncestorContainer;
	}

	protected getDOMElement(): HTMLElement {
		return this.Range.startContainer as HTMLElement;
	}

	public getElementChild(childIndex: number): Node | BlazorDOMElement | null {
		let element: Node | null = this.Range.startContainer;
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
				if (element !== this.Range.startContainer) {
					// is a component ?
					let blazorDom = this.getComponentFromNode(element);
					if (blazorDom != null) {
						element = blazorDom.Range.endContainer;
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
		if (element.nodeType == Node.COMMENT_NODE) { // check for performance
			for (let index in this.browserRenderer.childComponentLocations) {
				let component = this.browserRenderer.childComponentLocations[index];
				if (component.isComponent() && component.Range.startContainer === element)
					return component;
			}

			// old code, question: is better?
			//let nodeValue: string = element.nodeValue!;
			//if (nodeValue.startsWith("blazor-component-start.")) {
			//	let componentId: number = parseInt(nodeValue.split(".")[1]);
			//	let blazorDom = this.br.childComponentLocations[componentId];
			//	return blazorDom;
			//}
		}
		return null;
	}

	public insertNodeIntoDOM(node: Node, childIndex: number) {
		let parentElement = this.getParentDOMElement();

		let realSibling = this.getElementChild(childIndex);
		if (realSibling === null) {
			parentElement.appendChild(node);
		}
		else {
			parentElement.insertBefore(node, realSibling as Node);
		}
	}

	public removeFromDom(childIndex: number) {
		const element = this.getElementChild(childIndex)!;

		if (element instanceof BlazorDOMElement) {
			// Adjust range to whole component
			element.Range.setStartBefore(element.Range.startContainer);
			element.Range.setEndAfter(element.Range.endContainer);

			// Clear whole range
			element.Range.deleteContents();
		}
		else {
			// Remove only the childindex-nth element
			element.parentElement!.removeChild(element as Node);
		}
	}

	public updateText(childIndex: number, newText: string | null) {
		const domTextNode = this.getElementChild(childIndex) as Text;
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
		const element = this.getElementChild(childIndex) as Element;
		element.removeAttribute(attributeName);
	}

	protected setAttribute(attributeName: string, attributeValue: string | null) {
		const toDomElement = this.Range.startContainer as Element;

		if (attributeValue == null) {
			// or better delete the attribute?
			toDomElement.setAttribute(
				attributeName,
				""
			);
		}
		else {
			toDomElement.setAttribute(
				attributeName,
				attributeValue
			);
		}
	}

	protected isDOMAttribute(attributeName: string, value: string | null): boolean {
		// default is true
		return true;
	}

	protected applyEvent(attributeName: string, componentId: number, eventHandlerId: number): boolean {
		const toDomElement = this.Range.startContainer as Element;
		const browserRendererId = this.browserRenderer.browserRendererId;

		// TODO: Instead of applying separate event listeners to each DOM element, use event delegation
		// and remove all the _blazor*Listener hacks
		switch (attributeName) {
			case 'onclick': {
				toDomElement.removeEventListener('click', toDomElement['_blazorClickListener']);
				const listener = evt => raiseEvent(evt, browserRendererId, componentId, eventHandlerId, 'mouse', { Type: 'click' });
				toDomElement['_blazorClickListener'] = listener;
				toDomElement.addEventListener('click', listener);
				return true;
			}
		}
		return false;
	}

	public onDOMUpdating() { }
	public onDOMUpdated() { }

	public dispose() {
		this.Range.detach();
	}
}

class BlazorDOMComponent extends BlazorDOMElement {
	ComponentID: number;

	constructor(CID: number, parent: BlazorDOMElement, childIndex: number, br: BrowserRenderer) {
		const markerStart = document.createComment('blazor-component-start.' + CID);
		const markerEnd = document.createComment('blazor-component-end.' + CID);

		parent.insertNodeIntoDOM(markerEnd, childIndex);
		parent.insertNodeIntoDOM(markerStart, childIndex);

		super(br, markerStart, markerEnd);
		this.ComponentID = CID;
	}
}

class BlazorINPUTElement extends BlazorDOMElement {
	private handleSelectValue: string | null = null;

	protected isDOMAttribute(attributeName: string, value: string | null): boolean {
		const element = this.getDOMElement();

		if (attributeName == "value") {
			// Certain elements have built-in behaviour for their 'value' property
			switch (element.tagName) {
				case 'INPUT':
				case 'SELECT':
				case 'TEXTAREA':
					if (this.isCheckbox(element)) {
						(element as HTMLInputElement).checked = value === 'True';
					} else {
						this.handleSelectValue = value;
						(element as any).value = value;
					}
					return false;
				default:
					return super.isDOMAttribute(attributeName, value);
			}
		}
		else
			return true;
	}

	public onDOMUpdating() {
		this.handleSelectValue = null;
	}

	public onDOMUpdated() {
		if (this.handleSelectValue !== null) {
			const element = this.getDOMElement();
			if (element.tagName == "SELECT") {
				(element as any).value = this.handleSelectValue;
				this.handleSelectValue = null;
			}
		}
	}

	private isCheckbox(element: Element) {
		return element.tagName === 'INPUT' && element.getAttribute('type') === 'checkbox';
	}

	protected applyEvent(attributeName: string, componentId: number, eventHandlerId: number): boolean {
		const toDomElement = this.getDOMElement();
		const browserRendererId = this.browserRenderer.browserRendererId;

		// TODO: Instead of applying separate event listeners to each DOM element, use event delegation
		// and remove all the _blazor*Listener hacks
		switch (attributeName) {
			case 'onchange': {
				toDomElement.removeEventListener('change', toDomElement['_blazorChangeListener']);
				const targetIsCheckbox = this.isCheckbox(toDomElement);
				const listener = evt => {
					const newValue = targetIsCheckbox ? evt.target.checked : evt.target.value;
					raiseEvent(evt, browserRendererId, componentId, eventHandlerId, 'change', { Type: 'change', Value: newValue });
				};
				toDomElement['_blazorChangeListener'] = listener;
				toDomElement.addEventListener('change', listener);
				return true;
			}
			case 'onkeypress': {
				toDomElement.removeEventListener('keypress', toDomElement['_blazorKeypressListener']);
				const listener = evt => {
					// This does not account for special keys nor cross-browser differences. So far it's
					// just to establish that we can pass parameters when raising events.
					// We use C#-style PascalCase on the eventInfo to simplify deserialization, but this could
					// change if we introduced a richer JSON library on the .NET side.
					raiseEvent(evt, browserRendererId, componentId, eventHandlerId, 'keyboard', { Type: evt.type, Key: (evt as any).key });
				};
				toDomElement['_blazorKeypressListener'] = listener;
				toDomElement.addEventListener('keypress', listener);
				return true;
			}
		}
		return super.applyEvent(attributeName, componentId, eventHandlerId);
	}
}

export class BrowserRenderer {
	// private is better (todo: I don't like it!)
	public readonly childComponentLocations: { [componentId: number]: BlazorDOMElement } = {};

	public readonly browserRendererId: number;

	constructor(RendererId: number) {
		this.browserRendererId = RendererId;
	}

	public attachComponentToElement(componentId: number, element: Node) {
		let blazorElement = new BlazorDOMElement(this, element);
		this.childComponentLocations[componentId] = blazorElement;
	}

	private attachBlazorComponentToElement(componentId: number, element: BlazorDOMElement) {
		this.childComponentLocations[componentId] = element;
	}

	public updateComponent(componentId: number, edits: System_Array<RenderTreeEditPointer>, editsOffset: number, editsLength: number, referenceFrames: System_Array<RenderTreeFramePointer>) {
		const element = this.childComponentLocations[componentId];
		if (!element) {
			throw new Error(`No element is currently associated with component ${componentId}`);
		}

		this.applyEdits(componentId, element, 0, edits, editsOffset, editsLength, referenceFrames);
	}

	public disposeComponent(componentId: number) {
		this.childComponentLocations[componentId].dispose();
		delete this.childComponentLocations[componentId];
	}

	private applyEdits(componentId: number, parent: BlazorDOMElement, childIndex: number, edits: System_Array<RenderTreeEditPointer>, editsOffset: number, editsLength: number, referenceFrames: System_Array<RenderTreeFramePointer>) {
		let currentDepth = 0;
		let childIndexAtCurrentDepth = childIndex;
		const maxEditIndexExcl = editsOffset + editsLength;

		var parentElement = parent as BlazorDOMElement;

		var elementStack = new Array();
		elementStack.push(parentElement);

		for (let editIndex = editsOffset; editIndex < maxEditIndexExcl; editIndex++) {
			const edit = getRenderTreeEditPtr(edits, editIndex);
			const editType = renderTreeEdit.type(edit);
			switch (editType) {
				case EditType.prependFrame: {
					const frameIndex = renderTreeEdit.newTreeIndex(edit);
					const frame = getTreeFramePtr(referenceFrames, frameIndex);
					const siblingIndex = renderTreeEdit.siblingIndex(edit);
					this.insertFrame(componentId, parentElement, childIndexAtCurrentDepth + siblingIndex, referenceFrames, frame, frameIndex);
					break;
				}
				case EditType.removeFrame: {
					const siblingIndex = renderTreeEdit.siblingIndex(edit);
					this.removeNodeFromDOM(parentElement, childIndexAtCurrentDepth + siblingIndex);
					break;
				}
				case EditType.setAttribute: {
					const frameIndex = renderTreeEdit.newTreeIndex(edit);
					const frame = getTreeFramePtr(referenceFrames, frameIndex);
					const siblingIndex = renderTreeEdit.siblingIndex(edit);
					const element = parentElement.getElementChild(childIndexAtCurrentDepth + siblingIndex) as Element;

					const blazorElement = this.createBlazorDOMElement(element);
					blazorElement.applyAttribute(componentId, frame);
					blazorElement.dispose();
					break;
				}
				case EditType.removeAttribute: {
					const siblingIndex = renderTreeEdit.siblingIndex(edit);
					parent.removeAttribute(childIndexAtCurrentDepth + siblingIndex, renderTreeEdit.removedAttributeName(edit)!);
					break;
				}
				case EditType.updateText: {
					const frameIndex = renderTreeEdit.newTreeIndex(edit);
					const frame = getTreeFramePtr(referenceFrames, frameIndex);
					const siblingIndex = renderTreeEdit.siblingIndex(edit);
					parentElement.updateText(childIndexAtCurrentDepth + siblingIndex, renderTreeFrame.textContent(frame))
					break;
				}
				case EditType.stepIn: {
					const siblingIndex = renderTreeEdit.siblingIndex(edit);
					const stepInElement = parentElement.getElementChild(childIndexAtCurrentDepth + siblingIndex)!;

					elementStack.push(parentElement);
					if (stepInElement instanceof BlazorDOMElement == false) {
						// stepInElement is a simple DOM element, so create 
						parentElement = this.createBlazorDOMElement(stepInElement as HTMLElement);
					}
					parentElement.onDOMUpdating();

					currentDepth++;
					childIndexAtCurrentDepth = 0;
					break;
				}
				case EditType.stepOut: {
					parentElement.onDOMUpdated();
					if (parentElement.isComponent() == false) {
						// Dispose if a simple dom element (=BlazorDOMElement)
						parentElement.dispose();
					}

					parentElement = elementStack.pop();
					currentDepth--;
					childIndexAtCurrentDepth = currentDepth === 0 ? childIndex : 0; // The childIndex is only ever nonzero at zero depth
					break;
				}
				default: {
					const unknownType: never = editType; // Compile-time verification that the switch was exhaustive
					throw new Error(`Unknown edit type: ${unknownType}`);
				}
			}
		}
	}

	private insertFrame(componentId: number, parent: BlazorDOMElement, childIndex: number, frames: System_Array<RenderTreeFramePointer>, frame: RenderTreeFramePointer, frameIndex: number): number {
		const frameType = renderTreeFrame.frameType(frame);
		switch (frameType) {
			case FrameType.element:
				this.insertElement(componentId, parent, childIndex, frames, frame, frameIndex);
				return 1;
			case FrameType.text:
				this.insertText(parent, childIndex, frame);
				return 1;
			case FrameType.attribute:
				throw new Error('Attribute frames should only be present as leading children of element frames.');
			case FrameType.component:
				this.insertComponent(parent, childIndex, frame);
				return 1;
			case FrameType.region:
				return this.insertFrameRange(componentId, parent, childIndex, frames, frameIndex + 1, frameIndex + renderTreeFrame.subtreeLength(frame));
			default:
				const unknownType: never = frameType; // Compile-time verification that the switch was exhaustive
				throw new Error(`Unknown frame type: ${unknownType}`);
		}
	}

	private insertElement(componentId: number, parent: BlazorDOMElement, childIndex: number, frames: System_Array<RenderTreeFramePointer>, frame: RenderTreeFramePointer, frameIndex: number) {
		const tagName = renderTreeFrame.elementName(frame)!;
		const newDomElement = this.createElement(tagName, parent);
		parent.insertNodeIntoDOM(newDomElement, childIndex);

		let blazorElement = this.createBlazorDOMElement(newDomElement);

		// Apply attributes
		const descendantsEndIndexExcl = frameIndex + renderTreeFrame.subtreeLength(frame);
		for (let descendantIndex = frameIndex + 1; descendantIndex < descendantsEndIndexExcl; descendantIndex++) {
			const descendantFrame = getTreeFramePtr(frames, descendantIndex);

			if (renderTreeFrame.frameType(descendantFrame) === FrameType.attribute) {
				blazorElement.applyAttribute(componentId, descendantFrame);
			} else {
				// As soon as we see a non-attribute child, all the subsequent child frames are
				// not attributes, so bail out and insert the remnants recursively
				this.insertFrameRange(componentId, blazorElement, 0, frames, descendantIndex, descendantsEndIndexExcl);
				break;
			}
		}

		blazorElement.onDOMUpdated();
		blazorElement.dispose();
	}

	private insertComponent(parent: BlazorDOMElement, childIndex: number, frame: RenderTreeFramePointer) {
		// All we have to do is associate the child component ID with its location. We don't actually
		// do any rendering here, because the diff for the child will appear later in the render batch.
		const childComponentId = renderTreeFrame.componentId(frame);
		const component = this.createBlazorDOMComponent(childComponentId, parent, childIndex);
		this.attachBlazorComponentToElement(childComponentId, component);
	}

	private insertText(parent: BlazorDOMElement, childIndex: number, textFrame: RenderTreeFramePointer) {
		const textContent = renderTreeFrame.textContent(textFrame)!;
		const newDomTextNode = document.createTextNode(textContent);
		parent.insertNodeIntoDOM(newDomTextNode, childIndex);
	}

	private insertFrameRange(componentId: number, parent: BlazorDOMElement, childIndex: number, frames: System_Array<RenderTreeFramePointer>, startIndex: number, endIndexExcl: number): number {
		const origChildIndex = childIndex;
		for (let index = startIndex; index < endIndexExcl; index++) {
			const frame = getTreeFramePtr(frames, index);
			const numChildrenInserted = this.insertFrame(componentId, parent, childIndex, frames, frame, index);
			childIndex += numChildrenInserted;

			// Skip over any descendants, since they are already dealt with recursively
			const subtreeLength = renderTreeFrame.subtreeLength(frame);
			if (subtreeLength > 1) {
				index += subtreeLength - 1;
			}
		}

		return (childIndex - origChildIndex); // Total number of children inserted
	}

	private removeNodeFromDOM(parent: BlazorDOMElement, childIndex: number) {
		parent.removeFromDom(childIndex);
	}

	private createElement(tagName: string, parentElement: BlazorDOMElement): Element {
		const parent = parentElement.getParentDOMElement();
		const newDomElement = tagName === 'svg' || parent.namespaceURI === 'http://www.w3.org/2000/svg' ?
			document.createElementNS('http://www.w3.org/2000/svg', tagName) :
			document.createElement(tagName);
		return newDomElement;
	}

	private createBlazorDOMComponent(componentId: number, parent: BlazorDOMElement, childIndex: number): BlazorDOMComponent {
		// here can insert a lookup to a registry
		let blazorElement = new BlazorDOMComponent(componentId, parent, childIndex, this);
		return blazorElement;
	}

	private createBlazorDOMElement(stepInElement: Element): BlazorDOMElement {
		// here can insert a lookup to a registry
		if (stepInElement.tagName == "INPUT" || stepInElement.tagName == "SELECT" || stepInElement.tagName == "TEXTAREA")
			return new BlazorINPUTElement(this, stepInElement);
		else
			return new BlazorDOMElement(this, stepInElement);
	}
}

function raiseEvent(event: Event, browserRendererId: number, componentId: number, eventHandlerId: number, eventInfoType: EventInfoType, eventInfo: any) {
	event.preventDefault();

	if (!raiseEventMethod) {
		raiseEventMethod = platform.findMethod(
			'Microsoft.AspNetCore.Blazor.Browser', 'Microsoft.AspNetCore.Blazor.Browser.Rendering', 'BrowserRendererEventDispatcher', 'DispatchEvent'
		);
	}

	const eventDescriptor = {
		BrowserRendererId: browserRendererId,
		ComponentId: componentId,
		EventHandlerId: eventHandlerId,
		EventArgsType: eventInfoType
	};

	platform.callMethod(raiseEventMethod, null, [
		platform.toDotNetString(JSON.stringify(eventDescriptor)),
		platform.toDotNetString(JSON.stringify(eventInfo))
	]);
}

type EventInfoType = 'mouse' | 'keyboard' | 'change';
