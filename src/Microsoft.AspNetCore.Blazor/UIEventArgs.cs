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
    }

    /// <summary>
    /// Supplies information about a mouse event that is being raised.
    /// </summary>
    public class UIPointerEventArgs : UIMouseEventArgs
    {
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
