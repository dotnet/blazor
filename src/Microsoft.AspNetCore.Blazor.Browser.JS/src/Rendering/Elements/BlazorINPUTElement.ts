import { BlazorDOMElement } from './BlazorDOMElement';

export class BlazorINPUTElement extends BlazorDOMElement {
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

    //protected applyEvent(attributeName: string, componentId: number, eventHandlerId: number): boolean {
    //	const toDomElement = this.getDOMElement();
    //	const browserRendererId = this.browserRenderer.browserRendererId;

    //	// TODO: Instead of applying separate event listeners to each DOM element, use event delegation
    //	// and remove all the _blazor*Listener hacks
    //	switch (attributeName) {
    //		case 'onchange': {
    //			toDomElement.removeEventListener('change', toDomElement['_blazorChangeListener']);
    //			const targetIsCheckbox = this.isCheckbox(toDomElement);
    //			const listener = evt => {
    //				const newValue = targetIsCheckbox ? evt.target.checked : evt.target.value;
    //				raiseEvent(evt, browserRendererId, componentId, eventHandlerId, 'change', { Type: 'change', Value: newValue });
    //			};
    //			toDomElement['_blazorChangeListener'] = listener;
    //			toDomElement.addEventListener('change', listener);
    //			return true;
    //		}
    //		case 'onkeypress': {
    //			toDomElement.removeEventListener('keypress', toDomElement['_blazorKeypressListener']);
    //			const listener = evt => {
    //				// This does not account for special keys nor cross-browser differences. So far it's
    //				// just to establish that we can pass parameters when raising events.
    //				// We use C#-style PascalCase on the eventInfo to simplify deserialization, but this could
    //				// change if we introduced a richer JSON library on the .NET side.
    //				raiseEvent(evt, browserRendererId, componentId, eventHandlerId, 'keyboard', { Type: evt.type, Key: (evt as any).key });
    //			};
    //			toDomElement['_blazorKeypressListener'] = listener;
    //			toDomElement.addEventListener('keypress', listener);
    //			return true;
    //		}
    //	}
    //	return super.applyEvent(attributeName, componentId, eventHandlerId);
    //}
}