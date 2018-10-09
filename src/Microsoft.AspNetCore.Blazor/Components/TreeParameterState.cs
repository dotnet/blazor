// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.Rendering;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNetCore.Blazor.Components
{
    internal readonly struct TreeParameterState
    {
        private readonly static IDictionary<Type, ReflectedTreeParameterInfo[]> _cachedInfos
            = new ConcurrentDictionary<Type, ReflectedTreeParameterInfo[]>();

        public string LocalName { get; }
        public ProviderBase FromProvider { get; }

        public TreeParameterState(string localName, ProviderBase fromProvider)
        {
            LocalName = localName;
            FromProvider = fromProvider;
        }

        public static IReadOnlyList<TreeParameterState> FindTreeParameters(ComponentState componentState)
        {
            var componentType = componentState.Component.GetType();
            var infos = GetReflectedTreeParameterInfos(componentType);

            // For components known not to have any tree parameters, bail out early
            if (infos == null)
            {
                return null;
            }

            // Now try to find matches for each of the tree parameters
            // Defer instantiation of the result list until we know there's at least one
            List<TreeParameterState> resultStates = null;

            var numInfos = infos.Length;
            for (var infoIndex = 0; infoIndex < numInfos; infoIndex++)
            {
                ref var info = ref infos[infoIndex];
                if (TryGetMatchingProvider(info, componentState, out var provider))
                {
                    if (resultStates == null)
                    {
                        // Although not all parameters might be matched, we know the maximum number
                        resultStates = new List<TreeParameterState>(infos.Length - infoIndex);
                    }

                    resultStates.Add(new TreeParameterState(info.LocalName, provider));
                }
            }

            return resultStates;
        }

        private static bool TryGetMatchingProvider(in ReflectedTreeParameterInfo info, ComponentState componentState, out ProviderBase provider)
        {
            do
            {
                if (componentState.Component is ProviderBase candidateProvider
                    && candidateProvider.CanSupplyValue(info.ValueType, info.ProviderName))
                {
                    provider = candidateProvider;
                    return true;
                }

                componentState = componentState.ParentComponentState;
            } while (componentState != null);

            // No match
            provider = null;
            return false;
        }

        private static ReflectedTreeParameterInfo[] GetReflectedTreeParameterInfos(Type componentType)
        {
            if (!_cachedInfos.TryGetValue(componentType, out var infos))
            {
                infos = CreateReflectedTreeParameterInfos(componentType);
                _cachedInfos[componentType] = infos;
            }

            return infos;
        }

        private static ReflectedTreeParameterInfo[] CreateReflectedTreeParameterInfos(Type componentType)
        {
            List<ReflectedTreeParameterInfo> result = null;
            var candidateProps = ParameterCollectionExtensions.GetBindableProperties(componentType);
            foreach (var prop in candidateProps)
            {
                var parameterAttribute = prop.GetCustomAttribute<ParameterAttribute>();
                if (parameterAttribute.FromTree)
                {
                    if (result == null)
                    {
                        result = new List<ReflectedTreeParameterInfo>();
                    }

                    result.Add(new ReflectedTreeParameterInfo(
                        prop.Name,
                        prop.PropertyType,
                        parameterAttribute.ProviderName));
                }
            }

            return result?.ToArray();
        }

        readonly struct ReflectedTreeParameterInfo
        {
            public string LocalName { get; }
            public string ProviderName { get; }
            public Type ValueType { get; }

            public ReflectedTreeParameterInfo(
                string localName, Type valueType, string providerName)
            {
                LocalName = localName;
                ProviderName = providerName;
                ValueType = valueType;
            }
        }
    }
}
