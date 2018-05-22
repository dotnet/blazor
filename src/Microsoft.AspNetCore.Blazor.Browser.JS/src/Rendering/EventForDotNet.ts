export class EventForDotNet<TData extends UIEventArgs> {
  constructor(public readonly type: EventArgsType, public readonly data: TData) {
  }

  static fromDOMEvent(event: Event): EventForDotNet<UIEventArgs> {
    const element = event.target as Element;
    switch (event.type) {

      case 'change': {
        const targetIsCheckbox = isCheckbox(element);
        const newValue = targetIsCheckbox ? !!element['checked'] : element['value'];
        return new EventForDotNet<UIChangeEventArgs>('change', { type: event.type, value: newValue });
      }

      case 'copy':
      case 'cut':
      case 'paste':
        return new EventForDotNet<UIClipboardEventArgs>('clipboard', { type: event.type });

      case 'drag':
      case 'dragend':
      case 'dragenter':
      case 'dragleave':
      case 'dragover':
      case 'dragstart':
      case 'drop':
        return new EventForDotNet<UIDragEventArgs>('drag', { type: event.type });

      case 'error':
        return new EventForDotNet<UIProgressEventArgs>('error', { type: event.type });

      case 'focus':
      case 'blur':
      case 'focusin':
      case 'focusout':
        return new EventForDotNet<UIFocusEventArgs>('focus', { type: event.type });

      case 'keydown':
      case 'keyup':
      case 'keypress':
        return new EventForDotNet<UIKeyboardEventArgs>('keyboard', { type: event.type, key: (event as any).key });

      case 'contextmenu':
      case 'click':
      case 'mouseover':
      case 'mouseout':
      case 'mousemove':
      case 'mousedown':
      case 'mouseup':
      case 'dblclick':
        return new EventForDotNet<UIMouseEventArgs>('mouse', { type: event.type });

      case 'progress':
        return new EventForDotNet<UIProgressEventArgs>('progress', { type: event.type });

      case 'touchcancel':
      case 'touchend':
      case 'touchmove':
      case 'touchstart':
        return new EventForDotNet<UITouchEventArgs>('touch', { type: event.type });

      case 'gotpointercapture':
      case 'lostpointercapture':
      case 'pointercancel':
      case 'pointerdown':
      case 'pointerenter':
      case 'pointerleave':
      case 'pointermove':
      case 'pointerout':
      case 'pointerover':
      case 'pointerup':
        return new EventForDotNet<UIPointerEventArgs>('pointer', { type: event.type });

      case 'mousewheel':
        return new EventForDotNet<UIWheelEventArgs>('wheel', { type: event.type });


      default:
        return new EventForDotNet<UIEventArgs>('unknown', { type: event.type });
    }
  }
}

function isCheckbox(element: Element | null) {
  return element && element.tagName === 'INPUT' && element.getAttribute('type') === 'checkbox';
}

// The following interfaces must be kept in sync with the UIEventArgs C# classes

type EventArgsType = 'change' | 'clipboard' | 'drag' | 'error' | 'focus' | 'keyboard' | 'mouse' | 'pointer' | 'progress' | 'touch' | 'unknown' | 'wheel';

export interface UIEventArgs {
  type: string;
}

interface UIChangeEventArgs extends UIEventArgs {
  value: string | boolean;
}

interface UIClipboardEventArgs extends UIEventArgs {
}

interface UIDragEventArgs extends UIEventArgs {
}

interface UIErrorEventArgs extends UIEventArgs {
}

interface UIFocusEventArgs extends UIEventArgs {
}

interface UIKeyboardEventArgs extends UIEventArgs {
  key: string;
}

interface UIMouseEventArgs extends UIEventArgs {
}

interface UIPointerEventArgs extends UIMouseEventArgs {
}

interface UIProgressEventArgs extends UIEventArgs {
}

interface UITouchEventArgs extends UIEventArgs {
}

interface UIWheelEventArgs extends UIEventArgs {
}
