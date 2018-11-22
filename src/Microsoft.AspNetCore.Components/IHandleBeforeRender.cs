// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Components
{
    /// <summary>
    /// Interface implemented by components that receive notification that they are about to be rendered.
    /// </summary>
    public interface IHandleBeforeRender
    {
        /// <summary>
        /// Notifies the component that it is about to be rendered.
        /// </summary>
        void OnBeforeRender();
    }
}
