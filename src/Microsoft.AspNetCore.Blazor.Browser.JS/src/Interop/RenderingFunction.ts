const registeredCustomTags: { [identifier: string]: Function | undefined } = {};

export function registerCustomTag(identifier: string, implementation: Function) {
	registeredCustomTags[identifier] = implementation;
}

export function getRegisteredCustomTag(identifier: string): Function | null {
	const result = registeredCustomTags[identifier];
	if (result) {
		return result;
	} else {
		return null;
	}
}


const registeredCustomElement: { [identifier: number]: Function | undefined } = {};

export function registerCustomDOMElement(identifier: number, implementation: Function) {
	registeredCustomElement[identifier] = implementation;
}

export function getRegisteredCustomDOMElement(identifier: number): Function | null {
	const result = registeredCustomElement[identifier];
	if (result) {
		return result;
	} else {
		return null;
	}
}
