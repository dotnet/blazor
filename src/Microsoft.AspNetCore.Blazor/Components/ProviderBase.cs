// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.Rendering;
using System;

namespace Microsoft.AspNetCore.Blazor.Components
{
    /// <summary>
    /// Non-generic base class for <see cref="Provider{T}"/>.
    /// </summary>
    public abstract class ProviderBase
    {
        // This base class exists only so that TreeParameterState has a way
        // to work with all Provider types regardless of their generic param.

        internal abstract bool CanSupplyValue(Type valueType, string providerName);

        internal abstract object CurrentValue { get; }

        internal abstract void Subscribe(ComponentState subscriber);

        internal abstract void Unsubscribe(ComponentState subscriber);
    }
}
