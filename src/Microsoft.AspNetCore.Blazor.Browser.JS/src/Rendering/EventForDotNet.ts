export class EventForDotNet<TData extends UIEventArgs> {
  constructor(public readonly type: EventArgsType, public readonly data: TData) {
  }

  static fromDOMEvent(event: Event): EventForDotNet<UIEventArgs> {
    const element = event.target as Element;
    switch (event.type) {

      case 'change': {
        const targetIsCheckbox = isCheckbox(element);
        const newValue = targetIsCheckbox ? !!element['checked'] : element['value'];
        return new EventForDotNet<UIChangeEventArgs>('change', { Type: event.type, Value: newValue });
      }

      case 'copy':
      case 'cut':
      case 'paste':
        return new EventForDotNet<UIClipboardEventArgs>('clipboard', { Type: event.type });

      case 'drag':
      case 'dragend':
      case 'dragenter':
      case 'dragleave':
      case 'dragover':
      case 'dragstart':
      case 'drop':
        return new EventForDotNet<UIDragEventArgs>('drag', { Type: event.type });

      case 'error':
        return new EventForDotNet<UIProgressEventArgs>('error', { Type: event.type });

      case 'focus':
      case 'blur':
      case 'focusin':
      case 'focusout':
        return new EventForDotNet<UIFocusEventArgs>('focus', { Type: event.type });

      case 'keydown':
      case 'keyup':
      case 'keypress':
        return new EventForDotNet<UIKeyboardEventArgs>('keyboard', parseKeyboardEvent(event));

      case 'contextmenu':
      case 'click':
      case 'mouseover':
      case 'mouseout':
      case 'mousemove':
      case 'mousedown':
      case 'mouseup':
      case 'dblclick':
        return new EventForDotNet<UIMouseEventArgs>('mouse', parseMouseEvent(event));

      case 'progress':
        return new EventForDotNet<UIProgressEventArgs>('progress', { Type: event.type });

      case 'touchcancel':
      case 'touchend':
      case 'touchmove':
      case 'touchstart':
        return new EventForDotNet<UITouchEventArgs>('touch', { Type: event.type });

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
        return new EventForDotNet<UIPointerEventArgs>('pointer', parsePointerEvent(event));

      case 'mousewheel':
        return new EventForDotNet<UIWheelEventArgs>('wheel', { Type: event.type });


      default:
        return new EventForDotNet<UIEventArgs>('unknown', { Type: event.type });
    }
  }
}

function parseKeyboardEvent(event: any) {
  return {
    Type: event.type,
    Char: event.char,
    Key: event.key,
    Code: event.code,
    Location: event.location,
    Repeat: event.repeat,
    Locale: event.locale,
    CtrlKey: event.ctrlKey,
    ShiftKey: event.shiftKey,
    AltKey: event.altKey,
    MetaKey: event.metaKey
  }
}

function parsePointerEvent(event: any) {
  return Object.assign(parseMouseEvent(event),
    {
      PointerId: event.pointerId,
      Width: event.width,
      Height: event.height,
      Pressure: event.pressure,
      TiltX: event.tiltX,
      TiltY: event.tiltY,
      PointerType: event.pointerType,
      IsPrimary: event.isPrimary
    });
}

function parseMouseEvent(event: any) {
  return {
    Type: event.type,
    Detail: event.detail,
    ScreenX: event.screenX,
    ScreenY: event.screenY,
    ClientX: event.clientX,
    ClientY: event.clientY,
    Button: event.button,
    Buttons: event.buttons,
    MozPressure: event.mozPressure,
    CtrlKey: event.ctrlKey,
    ShiftKey: event.shiftKey,
    AltKey: event.altKey,
    MetaKey: event.metaKey
  }
}

function isCheckbox(element: Element | null) {
  return element && element.tagName === 'INPUT' && element.getAttribute('type') === 'checkbox';
}

// The following interfaces must be kept in sync with the UIEventArgs C# classes

type EventArgsType = 'change' | 'clipboard' | 'drag' | 'error' | 'focus' | 'keyboard' | 'mouse' | 'pointer' | 'progress' | 'touch' | 'unknown' | 'wheel';

export interface UIEventArgs {
  Type: string;
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
  Char: string;
  Key: string;
  Code: string;
  Location: number;
  Repeat: boolean;
  Locale: string;
  CtrlKey: boolean;
  ShiftKey: boolean;
  AltKey: boolean;
  MetaKey: boolean;
}

interface UIMouseEventArgs extends UIEventArgs {
  Detail: number;
  ScreenX: number;
  ScreenY: number;
  ClientX: number;
  ClientY: number;
  Button: number;
  Buttons: number;
  MozPressure: number;
  CtrlKey: boolean;
  ShiftKey: boolean;
  AltKey: boolean;
  MetaKey: boolean;
}

interface UIPointerEventArgs extends UIMouseEventArgs {
  PointerId: number;
  Width: number;
  Height: number;
  Pressure: number;
  TiltX: number;
  TiltY: number;
  PointerType: string;
  IsPrimary: boolean;
}

interface UIProgressEventArgs extends UIEventArgs {
}

interface UITouchEventArgs extends UIEventArgs {
}

interface UIWheelEventArgs extends UIEventArgs {
}
