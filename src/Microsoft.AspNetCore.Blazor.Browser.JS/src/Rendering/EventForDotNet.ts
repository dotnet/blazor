export class EventForDotNet<TData extends UIEventArgs> {
  constructor(public readonly type: EventArgsType, public readonly data: TData) {
  }

  static fromDOMEvent(event: Event): EventForDotNet<UIEventArgs> {
    const element = event.target as Element;
    switch (event.type) {

      case 'change': {
        const targetIsCheckbox = isCheckbox(element);
        const newValue = targetIsCheckbox ? !!element['checked'] : element['value'];
        return new EventForDotNet<UIChangeEventArgs>('change', {
          Type: event.type,
          Bubbles: event.bubbles,
          Cancelable: event.cancelable,
          Composed: (event as any).composed,
          Value: newValue
        });
      }

      case 'copy':
      case 'cut':
      case 'paste': {
        return new EventForDotNet<UIClipboardEventArgs>('clipboard', {
          Type: event.type,
          Bubbles: event.bubbles,
          Cancelable: event.cancelable,
          Composed: (event as any).composed
        });
      }

      case 'drag':
      case 'dragend':
      case 'dragenter':
      case 'dragleave':
      case 'dragover':
      case 'dragstart':
      case 'drop': {
        return new EventForDotNet<UIDragEventArgs>('clipboard', {
          Type: event.type,
          Bubbles: event.bubbles,
          Cancelable: event.cancelable,
          Composed: (event as any).composed
        });
      }

      case 'error': {
        return new EventForDotNet<UIProgressEventArgs>('error', {
          Type: event.type,
          Bubbles: event.bubbles,
          Cancelable: event.cancelable,
          Composed: (event as any).composed
        });
      }

      case 'focus':
      case 'blur':
      case 'focusin':
      case 'focusout': {
        return new EventForDotNet<UIFocusEventArgs>('focus', {
          Type: event.type,
          Bubbles: event.bubbles,
          Cancelable: event.cancelable,
          Composed: (event as any).composed
        });
      }

      case 'keydown':
      case 'keyup':
      case 'keypress': {
        return new EventForDotNet<UIKeyboardEventArgs>('keyboard', {
          Type: event.type,
          Bubbles: event.bubbles,
          Cancelable: event.cancelable,
          Composed: (event as any).composed,
          Key: (event as any).key,
          Code: (event as any).code,
          Location: (event as any).location,
          CtrlKey: (event as any).ctrlKey,
          ShiftKey: (event as any).shiftKey,
          AltKey: (event as any).altKey,
          MetaKey: (event as any).metaKey,
          Repeat: (event as any).repeat,
          IsComposing: (event as any).isComposing
        });
      }
        

      case 'contextmenu':
      case 'click':
      case 'mouseover':
      case 'mouseout':
      case 'mousemove':
      case 'mousedown':
      case 'mouseup':
      case 'dblclick': {
        return new EventForDotNet<UIMouseEventArgs>('mouse', {
          Type: event.type,
          Bubbles: event.bubbles,
          Cancelable: event.cancelable,
          Composed: (event as any).composed,
          ScreenX: (event as any).screenX,
          ScreenY: (event as any).screenY,
          ClientX: (event as any).clientX,
          ClientY: (event as any).clientY,
          CtrlKey: (event as any).ctrlKey,
          ShiftKey: (event as any).shiftKey,
          AltKey: (event as any).altKey,
          MetaKey: (event as any).metaKey,
          Button: (event as any).button,
          Buttons: (event as any).buttons,
          RelatedTarget: (event as any).relatedTarget,
          Region: (event as any).region
        });
      }

      case 'progress': {
        return new EventForDotNet<UIProgressEventArgs>('progress', {
          Type: event.type,
          Bubbles: event.bubbles,
          Cancelable: event.cancelable,
          Composed: (event as any).composed
        });
      }

      case 'touchcancel':
      case 'touchend':
      case 'touchmove':
      case 'touchstart': {
        return new EventForDotNet<UITouchEventArgs>('touch', {
          Type: event.type,
          Bubbles: event.bubbles,
          Cancelable: event.cancelable,
          Composed: (event as any).composed
        });
      }

      case 'gotpointercapture':
      case 'lostpointercapture':
      case 'pointercancel':
      case 'pointerdown':
      case 'pointerenter':
      case 'pointerleave':
      case 'pointermove':
      case 'pointerout':
      case 'pointerover':
      case 'pointerup': {
        return new EventForDotNet<UIPointerEventArgs>('pointer', {
          Type: event.type,
          Bubbles: event.bubbles,
          Cancelable: event.cancelable,
          Composed: (event as any).composed,
          ScreenX: (event as any).screenX,
          ScreenY: (event as any).screenY,
          ClientX: (event as any).clientX,
          ClientY: (event as any).clientY,
          CtrlKey: (event as any).ctrlKey,
          ShiftKey: (event as any).shiftKey,
          AltKey: (event as any).altKey,
          MetaKey: (event as any).metaKey,
          Button: (event as any).button,
          Buttons: (event as any).buttons,
          RelatedTarget: (event as any).relatedTarget,
          Region: (event as any).region,
          PointerId: (event as any).pointerId,
          Width: (event as any).width,
          Height: (event as any).height,
          Pressure: (event as any).pressure,
          TangentialPressure: (event as any).tangentialPressure,
          TiltX: (event as any).tiltX,
          TiltY: (event as any).tiltY,
          Twist: (event as any).twist,
          PointerType: (event as any).pointerType,
          IsPrimary: (event as any).isPrimary,
        });
      }

      case 'mousewheel': {
        return new EventForDotNet<UIWheelEventArgs>('wheel', {
          Type: event.type,
          Bubbles: event.bubbles,
          Cancelable: event.cancelable,
          Composed: (event as any).composed
        });

      }

      default: {
        return new EventForDotNet<UIEventArgs>('unknown', {
          Type: event.type,
          Bubbles: event.bubbles,
          Cancelable: event.cancelable,
          Composed: (event as any).composed
        });
      }
    }
  }
}

function isCheckbox(element: Element | null) {
  return element && element.tagName === 'INPUT' && element.getAttribute('type') === 'checkbox';
}

// The following interfaces must be kept in sync with the UIEventArgs C# classes

type EventArgsType = 'change' | 'clipboard' | 'drag' | 'error' | 'focus' | 'keyboard' | 'mouse' | 'pointer' | 'progress' | 'touch' | 'unknown' | 'wheel';

export interface UIEventArgs {
  Type: string;
  Bubbles: boolean;
  Cancelable: boolean;
  Composed: boolean;
}

interface UIChangeEventArgs extends UIEventArgs {
  Value: string | boolean;
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
  Key: string;
  Code: string;
  Location: number;
  CtrlKey: boolean;
  ShiftKey: boolean;
  AltKey: boolean;
  MetaKey: boolean;
  Repeat: boolean;
  IsComposing: boolean;
}

interface UIMouseEventArgs extends UIEventArgs {
  ScreenX: number;
  ScreenY: number;
  ClientX: number;
  ClientY: number;
  CtrlKey: boolean;
  ShiftKey: boolean;
  AltKey: boolean;
  MetaKey: boolean;
  Button: number;
  Buttons: number;
  RelatedTarget: EventTarget;
  Region: string;
}

interface UIPointerEventArgs extends UIMouseEventArgs {
  PointerId: number;
  Width: number;
  Height: number;
  Pressure: number;
  TangentialPressure: number;
  TiltX: number;
  TiltY: number;
  Twist: number;
  PointerType: string;
  IsPrimary: boolean;
}

interface UIProgressEventArgs extends UIEventArgs {
}

interface UITouchEventArgs extends UIEventArgs {
}

interface UIWheelEventArgs extends UIEventArgs {
}
