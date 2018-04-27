import { BrowserRenderer } from '../BrowserRenderer';
import { BlazorDOMElement } from './BlazorDOMElement';
import { BlazorINPUTElement } from './BlazorINPUTElement';

import { getRegisteredCustomTag, getRegisteredCustomDOMElement } from '../../Interop/RenderingFunction';

export class BlazorDOMComponent extends BlazorDOMElement {
    ComponentID: number;

    constructor(CID: number, parent: BlazorDOMElement, childIndex: number, br: BrowserRenderer) {
        const markerStart = document.createComment('blazor-component-start.' + CID);
        const markerEnd = document.createComment('blazor-component-end.' + CID);

        parent.insertNodeIntoDOM(markerEnd, childIndex);
        parent.insertNodeIntoDOM(markerStart, childIndex);

        super(br, markerStart, markerEnd);
        this.ComponentID = CID;
    }

    protected setAttribute(attributeName: string, attributeValue: string | null) {
        // Blazor DOM Component do not have attributes
    }
}

export function createBlazorDOMElement(br: BrowserRenderer, stepInElement: Element): BlazorDOMElement {
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