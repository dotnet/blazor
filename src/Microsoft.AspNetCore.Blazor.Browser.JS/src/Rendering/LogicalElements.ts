export function toLogicalElement(element: Element) {
  return element as any as LogicalElement;
}

export function createAndInsertLogicalContainer(parent: LogicalElement, childIndex: number): LogicalElement {
  const containerElement = isSvgElement(parent) ?
    document.createElementNS('http://www.w3.org/2000/svg', 'g') :
    document.createElement('blazor-component');
  insertLogicalChild(containerElement, parent, childIndex);
  return containerElement as any as LogicalElement;
}

export function insertLogicalChild(child: Node, parent: LogicalElement, childIndex: number) {
  const parentElement = parent as any as Element;
  if (childIndex >= parentElement.childNodes.length) {
    parentElement.appendChild(child);
  } else {
    parentElement.insertBefore(child, parentElement.childNodes[childIndex]);
  }
}

export function removeLogicalChild(parent: LogicalElement, childIndex: number) {
  const parentElement = parent as any as Element;
  parentElement.removeChild(parentElement.childNodes[childIndex]);
}

export function getLogicalParent(element: LogicalElement): LogicalElement | null {
  const suppliedElement = element as any as Element;
  return suppliedElement.parentElement as any as LogicalElement;
}

export function getLogicalChild(parent: LogicalElement, childIndex: number): LogicalElement {
  const parentElement = parent as any as Element;
  return parentElement.childNodes[childIndex] as any as LogicalElement;
}

export function isSvgElement(element: LogicalElement) {
  return getClosestDomElement(element).namespaceURI === 'http://www.w3.org/2000/svg';
}

function getClosestDomElement(logicalElement: LogicalElement) {
  if (logicalElement instanceof Element) {
    return logicalElement;
  } else if (logicalElement instanceof Comment) {
    return logicalElement.parentNode! as Element;
  } else {
    throw new Error('Not a valid logical element');
  }
}

// Nominal type to represent a logical element without needing to allocate any object for instances
export interface LogicalElement { LogicalElement__DO_NOT_IMPLEMENT: any };
