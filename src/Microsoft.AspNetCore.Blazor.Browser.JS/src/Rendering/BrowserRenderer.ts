import { System_Array, MethodHandle } from '../Platform/Platform';
import { getRenderTreeEditPtr, renderTreeEdit, RenderTreeEditPointer, EditType } from './RenderTreeEdit';
import { getTreeFramePtr, renderTreeFrame, FrameType, RenderTreeFramePointer } from './RenderTreeFrame';
import { platform } from '../Environment';
import { EventDelegator } from './EventDelegator';
import { EventForDotNet, UIEventArgs } from './EventForDotNet';

import { BlazorDOMElement } from './Elements/BlazorDOMElement'
import { createBlazorDOMElement, createBlazorDOMComponent } from './Elements/BlazorDOMComponent';

let raiseEventMethod: MethodHandle;
let renderComponentMethod: MethodHandle;

export class BrowserRenderer {
	// private is better (todo: I don't like it!)
	public eventDelegator: EventDelegator;
	public readonly childComponentLocations: { [componentId: number]: BlazorDOMElement } = {};

	public readonly browserRendererId: number;

	constructor(rendererId: number) {
		this.browserRendererId = rendererId;
		this.eventDelegator = new EventDelegator((event, componentId, eventHandlerId, eventArgs) => {
			raiseEvent(event, this.browserRendererId, componentId, eventHandlerId, eventArgs);
		});
	}

	public attachRootComponentToElement(componentId: number, element: Element) {
		this.attachComponentToElement(componentId, element);
	}

	private attachComponentToElement(componentId: number, element: Node) {
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
		parentElement.onDOMUpdating();

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

					const blazorElement = createBlazorDOMElement(this, element);
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
						parentElement = createBlazorDOMElement(this, stepInElement as HTMLElement);
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

		parentElement.onDOMUpdated();
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
				this.insertComponent(parent, childIndex, frame, frames, frameIndex);
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

		let blazorElement = createBlazorDOMElement(this, newDomElement);

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

	private insertComponent(parent: BlazorDOMElement, childIndex: number, frame: RenderTreeFramePointer, frames: System_Array<RenderTreeFramePointer>, frameIndex: number) {
		// All we have to do is associate the child component ID with its location. We don't actually
		// do any rendering here, because the diff for the child will appear later in the render batch.
		const childComponentId = renderTreeFrame.componentId(frame);
		const customComponentType = renderTreeFrame.customComponentType(frame);
		const blazorElement = createBlazorDOMComponent(this, childComponentId, parent, childIndex, customComponentType);
		this.attachBlazorComponentToElement(childComponentId, blazorElement);

		if (customComponentType != 0) {
			// Apply attributes
			const descendantsEndIndexExcl = frameIndex + renderTreeFrame.subtreeLength(frame);
			for (let descendantIndex = frameIndex + 1; descendantIndex < descendantsEndIndexExcl; descendantIndex++) {
				const descendantFrame = getTreeFramePtr(frames, descendantIndex);

				if (renderTreeFrame.frameType(descendantFrame) === FrameType.attribute) {
					blazorElement.applyAttribute(childComponentId, descendantFrame);
				} else {
					break;
				}
			}
		}
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

	public disposeEventHandler(eventHandlerId: number) {
		this.eventDelegator.removeListener(eventHandlerId);
	}

	private createElement(tagName: string, parentElement: BlazorDOMElement): Element {
		const parent = parentElement.getParentDOMElement();
		const newDomElement = tagName === 'svg' || parent.namespaceURI === 'http://www.w3.org/2000/svg' ?
			document.createElementNS('http://www.w3.org/2000/svg', tagName) :
			document.createElement(tagName);
		return newDomElement;
	}
}

export function raiseEvent(event: Event, browserRendererId: number, componentId: number, eventHandlerId: number, eventArgs: EventForDotNet<UIEventArgs>) {
	if (event.preventDefault !== undefined)
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
		EventArgsType: eventArgs.type
	};

	platform.callMethod(raiseEventMethod, null, [
		platform.toDotNetString(JSON.stringify(eventDescriptor)),
		platform.toDotNetString(JSON.stringify(eventArgs.data))
	]);
}
