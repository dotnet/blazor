import { BrowserRenderer } from '../BrowserRenderer';
import { renderTreeFrame, RenderTreeFramePointer } from '../RenderTreeFrame';

export class BlazorDOMElement {
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

    public removeFromDom(childIndex: number | null = null) {
        if (childIndex === null) {
            // Adjust range to whole component
            this.Range.setStartBefore(this.Range.startContainer);
            this.Range.setEndAfter(this.Range.endContainer);

            // Clear whole range
            this.Range.deleteContents();
        }
        else {
            const element = this.getElementChild(childIndex)!;

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
        this.Range.detach();
    }
}