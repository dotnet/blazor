import { System_Array, MethodHandle } from '../Platform/Platform';
import { getRenderTreeEditPtr, renderTreeEdit, RenderTreeEditPointer, EditType } from './RenderTreeEdit';
import { getTreeFramePtr, renderTreeFrame, FrameType, RenderTreeFramePointer } from './RenderTreeFrame';
import { platform } from '../Environment';
import { EventDelegator } from './EventDelegator';
import { EventForDotNet, UIEventArgs } from './EventForDotNet';
import { applyCaptureIdToElement } from './ElementReferenceCapture';

import { BlazorDOMElement } from './Elements/BlazorDOMElement';
import { createBlazorDOMComponent, createBlazorDOMElement } from './Elements/ElementCreators';

let raiseEventMethod: MethodHandle;
let renderComponentMethod: MethodHandle;

export class BrowserRenderer {
	// private is better (todo: I don't like it!)
	public eventDelegator: EventDelegator;
	private readonly childComponentLocations: { [componentId: number]: BlazorDOMElement } = {};

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
		this.attachBlazorComponentToElement(componentId, blazorElement);
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

	private attachBlazorComponentToElement(componentId: number, element: BlazorDOMElement) {
		this.childComponentLocations[componentId] = element;
	}

	private applyEdits(componentId: number, parent: BlazorDOMElement, childIndex: number, edits: System_Array<RenderTreeEditPointer>, editsOffset: number, editsLength: number, referenceFrames: System_Array<RenderTreeFramePointer>) {

		let currentDepth = 0;
		let childIndexAtCurrentDepth = childIndex;
		const maxEditIndexExcl = editsOffset + editsLength;

		parent.onDOMUpdating();

		var elementStack = new Array();
		elementStack.push(parent);

		for (let editIndex = editsOffset; editIndex < maxEditIndexExcl; editIndex++) {
			const edit = getRenderTreeEditPtr(edits, editIndex);
			const editType = renderTreeEdit.type(edit);
			switch (editType) {
				case EditType.prependFrame: {
					const frameIndex = renderTreeEdit.newTreeIndex(edit);
					const frame = getTreeFramePtr(referenceFrames, frameIndex);
					const siblingIndex = renderTreeEdit.siblingIndex(edit);
					this.insertFrame(componentId, parent, childIndexAtCurrentDepth + siblingIndex, referenceFrames, frame, frameIndex);
					break;
				}
				case EditType.removeFrame: {
					const siblingIndex = renderTreeEdit.siblingIndex(edit);
					this.removeNodeFromDOM(parent, childIndexAtCurrentDepth + siblingIndex);
					break;
				}
				case EditType.setAttribute: {
					const frameIndex = renderTreeEdit.newTreeIndex(edit);
					const frame = getTreeFramePtr(referenceFrames, frameIndex);
					const siblingIndex = renderTreeEdit.siblingIndex(edit);
					const element = parent.getLogicalChild(childIndexAtCurrentDepth + siblingIndex) as Element;

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
					parent.updateText(childIndexAtCurrentDepth + siblingIndex, renderTreeFrame.textContent(frame))
					break;
				}
				case EditType.stepIn: {
					const siblingIndex = renderTreeEdit.siblingIndex(edit);
					const stepInElement = parent.getLogicalChild(childIndexAtCurrentDepth + siblingIndex)!;

					elementStack.push(parent);
					// if stepInElement is a simple DOM element, create a element
					if (stepInElement instanceof BlazorDOMElement == false) {
						parent = createBlazorDOMElement(this, stepInElement as HTMLElement);
					}
					parent.onDOMUpdating();

					currentDepth++;
					childIndexAtCurrentDepth = 0;
					break;
				}
				case EditType.stepOut: {
					parent.onDOMUpdated();
					//if (parent.isComponent() == false) {
					//    // Dispose if a simple dom element (=BlazorDOMElement)
					//    parent.dispose();
					//}

					parent = elementStack.pop();
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

		parent.onDOMUpdated();
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
			case FrameType.elementReferenceCapture:
				{
					let parentElement = parent.getClosestDomElement() as Element; 
					if (parentElement instanceof Element) {
						applyCaptureIdToElement(parentElement, renderTreeFrame.elementReferenceCaptureId(frame));
						return 0; // A "capture" is a child in the diff, but has no node in the DOM
					} else {
						throw new Error('Reference capture frames can only be children of element frames.');
					}
				}
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

	private createElement(tagName: string, parentElement: BlazorDOMElement): Element {
		const parent = parentElement.getClosestDomElement();
		const newDomElement = tagName === 'svg' || parent.namespaceURI === 'http://www.w3.org/2000/svg' ?
			document.createElementNS('http://www.w3.org/2000/svg', tagName) :
			document.createElement(tagName);
		return newDomElement;
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
			index += this.countDescendantFrames(frame);
		}

		return (childIndex - origChildIndex); // Total number of children inserted
	}

	private removeNodeFromDOM(parent: BlazorDOMElement, childIndex: number) {
		parent.removeFromDom(childIndex);
	}

	public disposeEventHandler(eventHandlerId: number) {
		this.eventDelegator.removeListener(eventHandlerId);
	}

	private countDescendantFrames(frame: RenderTreeFramePointer): number {
		switch (renderTreeFrame.frameType(frame)) {
			// The following frame types have a subtree length. Other frames may use that memory slot
			// to mean something else, so we must not read it. We should consider having nominal subtypes
			// of RenderTreeFramePointer that prevent access to non-applicable fields.
			case FrameType.component:
			case FrameType.element:
			case FrameType.region:
				return renderTreeFrame.subtreeLength(frame) - 1;
			default:
				return 0;
		}
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
    browserRendererId,
    componentId,
    eventHandlerId,
    eventArgsType: eventArgs.type
  };

  let t0 = performance.now();

  platform.callMethod(raiseEventMethod, null, [
		platform.toDotNetString(JSON.stringify(eventDescriptor)),
		platform.toDotNetString(JSON.stringify(eventArgs.data))
  ]);

  let t1 = performance.now();
  console.log("BrowserRendererEventDispatcher took " + (t1 - t0) + " milliseconds.")
}
