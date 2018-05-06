// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Blazor
{
    /// <summary>
    /// Supplies information about an event that is being raised.
    /// </summary>
    public class UIEventArgs
    {
        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        public string Type { get; set; }
    }

    /// <summary>
    /// Supplies information about an input change event that is being raised.
    /// </summary>
    public class UIChangeEventArgs : UIEventArgs
    {
        /// <summary>
        /// Gets or sets the new value of the input. This may be a <see cref="string"/>
        /// or a <see cref="bool"/>.
        /// </summary>
        public object Value { get; set; }
    }

    /// <summary>
    /// Supplies information about an clipboard event that is being raised.
    /// </summary>
    public class UIClipboardEventArgs : UIEventArgs
    {
    }

    /// <summary>
    /// Supplies information about an drag event that is being raised.
    /// </summary>
    public class UIDragEventArgs : UIEventArgs
    {
    }

    /// <summary>
    /// Supplies information about an error event that is being raised.
    /// </summary>
    public class UIErrorEventArgs : UIEventArgs
    {
    }

    /// <summary>
    /// Supplies information about a focus event that is being raised.
    /// </summary>
    public class UIFocusEventArgs : UIEventArgs
    {
        // Not including support for 'relatedTarget' since we don't have a good way to represent it.
        // see: https://developer.mozilla.org/en-US/docs/Web/API/FocusEvent
    }

    /// <summary>
    /// Supplies information about a keyboard event that is being raised.
    /// </summary>
    public class UIKeyboardEventArgs : UIEventArgs
    {
        /// <summary>
        /// The character value of the key. If the key corresponds to a printable character, 
        /// this value is a non-empty Unicode string containing that character. 
        /// If the key doesn't have a printable representation, this is an empty string
        /// </summary>
        public string Char { get; set; }
        /// <summary>
        /// The key value of the key represented by the event. 
        /// If the value has a printed representation, this attribute's value is the same as the char attribute. 
        /// Otherwise, it's one of the key value strings specified in 'Key values'. 
        /// If the key can't be identified, this is the string "Unidentified"
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Holds a string that identifies the physical key being pressed. 
        /// The value is not affected by the current keyboard layout or modifier state, so a particular key will always return the same value.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The location of the key on the device.
        /// </summary>
        public float Location { get; set; }

        /// <summary>
        /// true if a key has been depressed long enough to trigger key repetition, otherwise false.
        /// </summary>
        public bool Repeat { get; set; }

        /// <summary>
        /// The language code for the key event, if available; otherwise, the empty string.
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// true if the control key was down when the event was fired. false otherwise.
        /// </summary>
        public bool CtrlKey { get; set; }

        /// <summary>
        /// true if the shift key was down when the event was fired. false otherwise.
        /// </summary>
        public bool ShiftKey { get; set; }

        /// <summary>
        /// true if the alt key was down when the event was fired. false otherwise.
        /// </summary>
        public bool AltKey { get; set; }

        /// <summary>
        /// true if the meta key was down when the event was fired. false otherwise.
        /// </summary>
        public bool MetaKey { get; set; }
    }

    /// <summary>
    /// Supplies information about a mouse event that is being raised.
    /// </summary>
    public class UIMouseEventArgs : UIEventArgs
    {
        /// <summary>
        /// A count of consecutive clicks that happened in a short amount of time, incremented by one.
        /// </summary>
        public float Detail { get; set; }

        /// <summary>
        /// The X coordinate of the mouse pointer in global (screen) coordinates.
        /// </summary>
        public long ScreenX { get; set; }

        /// <summary>
        /// The Y coordinate of the mouse pointer in global (screen) coordinates.
        /// </summary>
        public long ScreenY { get; set; }

        /// <summary>
        /// The X coordinate of the mouse pointer in local (DOM content) coordinates.
        /// </summary>
        public long ClientX { get; set; }

        /// <summary>
        /// 	The Y coordinate of the mouse pointer in local (DOM content) coordinates.
        /// </summary>
        public long ClientY { get; set; }

        /// <summary>
        /// The button number that was pressed when the mouse event was fired:
        /// Left button=0,
        /// middle button=1 (if present),
        /// right button=2.
        /// For mice configured for left handed use in which the button actions are reversed the values are instead read from right to left.
        /// </summary>
        public long Button { get; set; }

        /// <summary>
        /// The buttons being pressed when the mouse event was fired:
        /// Left button=1,
        /// Right button=2,
        /// Middle (wheel) button=4,
        /// 4th button (typically, "Browser Back" button)=8,
        /// 5th button (typically, "Browser Forward" button)=16.
        /// If two or more buttons are pressed, returns the logical sum of the values.
        /// E.g., if Left button and Right button are pressed, returns 3 (=1 | 2).
        /// </summary>
        public long Buttons { get; set; }

        /// <summary>
        /// The amount of pressure applied to a touch or tabdevice when generating the event;
        /// this value ranges between 0.0 (minimum pressure) and 1.0 (maximum pressure).
        /// </summary>
        public float MozPressure { get; set; }

        /// <summary>
        /// true if the control key was down when the event was fired. false otherwise.
        /// </summary>
        public bool CtrlKey { get; set; }

        /// <summary>
        /// true if the shift key was down when the event was fired. false otherwise.
        /// </summary>
        public bool ShiftKey { get; set; }

        /// <summary>
        /// true if the alt key was down when the event was fired. false otherwise.
        /// </summary>
        public bool AltKey { get; set; }

        /// <summary>
        /// true if the meta key was down when the event was fired. false otherwise.
        /// </summary>
        public bool MetaKey { get; set; }
    }

    /// <summary>
    /// Supplies information about a mouse event that is being raised.
    /// </summary>
    public class UIPointerEventArgs : UIMouseEventArgs
    {
        /// <summary>
        /// A unique identifier for the pointer causing the event.
        /// </summary>
        public string PointerId { get; set; }

        /// <summary>
        /// The width (magnitude on the X axis), in CSS pixels, of the contact geometry of the pointer.
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// The height (magnitude on the Y axis), in CSS pixels, of the contact geometry of the pointer.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// The normalized pressure of the pointer input in the range of 0 to 1,
        /// where 0 and 1 represent the minimum and maximum pressure the hardware is capable of detecting, respectively.
        /// </summary>
        public float Pressure { get; set; }

        /// <summary>
        /// The plane angle (in degrees, in the range of -90 to 90) between the Y-Z plane
        /// and the plane containing both the transducer (e.g. pen stylus) axis and the Y axis.
        /// </summary>
        public float TiltX { get; set; }

        /// <summary>
        /// The plane angle (in degrees, in the range of -90 to 90) between the X-Z plane
        /// and the plane containing both the transducer (e.g. pen stylus) axis and the X axis.
        /// </summary>
        public float TiltY { get; set; }

        /// <summary>
        /// Indicates the device type that caused the event.
        /// Must be one of the strings mouse, pen or touch, or an empty string.
        /// </summary>
        public string PointerType { get; set; }

        /// <summary>
        /// Indicates if the pointer represents the primary pointer of this pointer type.
        /// </summary>
        public bool IsPrimary { get; set; }
    }

    /// <summary>
    /// Supplies information about a progress event that is being raised.
    /// </summary>
    public class UIProgressEventArgs : UIMouseEventArgs
    {
    }

    /// <summary>
    /// Supplies information about a touch event that is being raised.
    /// </summary>
    public class UITouchEventArgs : UIEventArgs
    {
    }

    /// <summary>
    /// Supplies information about a mouse wheel event that is being raised.
    /// </summary>
    public class UIWheelEventArgs : UIEventArgs
    {
    }
}
