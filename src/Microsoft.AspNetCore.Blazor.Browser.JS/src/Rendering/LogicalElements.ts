/*
  A LogicalElement plays the same role as an Element instance from the point of view of the
  API consumer. Inserting and removing logical elements updates the browser DOM just the same.

  The difference is that, unlike regular DOM mutation APIs, the LogicalElement APIs don't use
  the underlying DOM structure as the data storage for the element hierarchy. Instead, the
  LogicalElement APIs take care of tracking hierarchical relationships separately. The point
  of this is to permit a logical tree structure in which parent/child relationships don't
  have to be materialized in terms of DOM element parent/child relationships. And the reason
  why we want that is so that hierarchies of Blazor components can be tracked even when those
  components' render output need not be a single literal DOM element.

  Consumers of the API don't need to know about the implementation, but how it's done is:
  - Each LogicalElement is materialized in the DOM as either:
    - A Node instance, for actual Node instances inserted using 'insertLogicalChild' or
      for Element instances promoted to LogicalElement via 'toLogicalElement'
    - A Comment instance, for 'logical container' instances inserted using 'createAndInsertLogicalContainer'
  - Then, on that instance (i.e., the Node or Comment), we store an array of 'logical children'
    instances, e.g.,
      [firstChild, secondChild, thirdChild, ...]
    ... plus we store a reference to the 'logical parent' (if any)
  - The 'logical children' array means we can look up in O(1):
    - The number of logical children (not currently implemented because not required, but trivial)
    - The logical child at any given index
  - Whenever a logical child is added or removed, we update the parent's array of logical children
*/

const logicalChildrenPropname = '_blazorLogicalChildren';
const logicalParentPropname = '_blazorLogicalParent';

export function toLogicalElement(element: Element) {
  if (element.childNodes.length > 0) {
    throw new Error('New logical elements must start empty');
  }

  element[logicalChildrenPropname] = [];
  return element as any as LogicalElement;
}

export function createAndInsertLogicalContainer(parent: LogicalElement, childIndex: number): LogicalElement {
  const containerElement = document.createComment('!');
  insertLogicalChild(containerElement, parent, childIndex);
  return containerElement as any as LogicalElement;
}

export function insertLogicalChild(child: Node, parent: LogicalElement, childIndex: number) {
  // For consistency with regular DOM APIs, moving an existing logical element automatically
  // removes it from its previous location.
  const childAsLogicalElement = child as any as LogicalElement;
  const previousLogicalParent = getLogicalParent(childAsLogicalElement);
  if (previousLogicalParent) {
    const previousSiblings = getLogicalChildrenArray(previousLogicalParent);
    const previousSiblingIndex = Array.prototype.indexOf.call(previousSiblings, child);
    previousSiblings.splice(previousSiblingIndex, 1);
  }

  // TODO: Allow for child being a Comment with its own logical children
  const newSiblings = getLogicalChildrenArray(parent);
  const newPhysicalParent = getClosestDomElement(parent);
  if (childIndex < newSiblings.length) {
    newPhysicalParent.insertBefore(child, newSiblings[childIndex] as any as Node);
    newSiblings.splice(childIndex, 0, childAsLogicalElement);
  } else {
    if (parent instanceof Comment) {
      const parentLogicalNextSibling = getLogicalNextSibling(parent);
      if (parentLogicalNextSibling) {
        newPhysicalParent.insertBefore(child, parentLogicalNextSibling as any as Node);
      } else {
        newPhysicalParent.appendChild(child);
      }
    } else {
      newPhysicalParent.appendChild(child);
    }

    newSiblings.push(childAsLogicalElement);
  }

  childAsLogicalElement[logicalParentPropname] = parent;
  if (!(logicalChildrenPropname in childAsLogicalElement)) {
    childAsLogicalElement[logicalChildrenPropname] = [];
  }
}

export function removeLogicalChild(parent: LogicalElement, childIndex: number) {
  const childrenArray = getLogicalChildrenArray(parent);
  const childToRemove = childrenArray.splice(childIndex, 1)[0];

  // If it's a logical container, also remove its descendants
  if (childToRemove instanceof Comment) {
    const grandchildrenArray = getLogicalChildrenArray(childToRemove);
    while (grandchildrenArray.length > 0) {
      removeLogicalChild(childToRemove, 0);
    }
  }

  // Finally, remove the node itself
  const domNodeToRemove = childToRemove as any as Node;
  domNodeToRemove.parentNode!.removeChild(domNodeToRemove);
}

export function getLogicalParent(element: LogicalElement): LogicalElement | null {
  return (element[logicalParentPropname] as LogicalElement) || null;
}

export function getLogicalChild(parent: LogicalElement, childIndex: number): LogicalElement {
  return getLogicalChildrenArray(parent)[childIndex];
}

export function isSvgElement(element: LogicalElement) {
  return getClosestDomElement(element).namespaceURI === 'http://www.w3.org/2000/svg';
}

function getLogicalChildrenArray(element: LogicalElement) {
  return element[logicalChildrenPropname] as LogicalElement[];
}

function getLogicalNextSibling(element: LogicalElement): LogicalElement | null {
  const siblings = getLogicalChildrenArray(getLogicalParent(element)!);
  const siblingIndex = Array.prototype.indexOf.call(siblings, element);
  return siblings[siblingIndex + 1] || null;
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
