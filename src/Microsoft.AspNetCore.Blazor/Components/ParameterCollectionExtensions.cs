// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.Reflection;
using Microsoft.AspNetCore.Blazor.RenderTree;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.Blazor.Components
{
    /// <summary>
    /// Extension methods for the <see cref="ParameterCollection"/> type.
    /// </summary>
    public static class ParameterCollectionExtensions
    {
        private const BindingFlags _bindablePropertyFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase;

        private readonly static IDictionary<Type, IDictionary<string, WriteParameterAction>> _cachedParameterWriters
            = new ConcurrentDictionary<Type, IDictionary<string, WriteParameterAction>>();

        /// <summary>
        /// Iterates through the <see cref="ParameterCollection"/>, assigning each parameter
        /// to a property of the same name on <paramref name="target"/>.
        /// </summary>
        /// <param name="parameterCollection">The <see cref="ParameterCollection"/>.</param>
        /// <param name="target">An object that has a public writable property matching each parameter's name and type.</param>
        public static void AssignToProperties(
            in this ParameterCollection parameterCollection,
            object target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var targetType = target.GetType();
            if (!_cachedParameterWriters.TryGetValue(targetType, out var parameterWriters))
            {
                parameterWriters = CreateParameterWriters(targetType);
                _cachedParameterWriters[targetType] = parameterWriters;
            }
            var usedWriters = new List<WriteParameterAction>();

            foreach (var parameter in parameterCollection)
            {
                var parameterName = parameter.Name;
                if (!parameterWriters.TryGetValue(parameterName, out var parameterWriter))
                {
                    ThrowForUnknownIncomingParameterName(targetType, parameterName);
                }

                try
                {
                    parameterWriter.Setter(ref parameter.Frame, target);
                    usedWriters.Add(parameterWriter);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Unable to set property '{parameterName}' on object of " +
                        $"type '{target.GetType().FullName}'. The error was: {ex.Message}", ex);
                }
            }
            foreach (var nonUsedParameter in parameterWriters.Values.Except(usedWriters))
            {
                var type = nonUsedParameter.PropertyType;
                var nullValueTreeFrame = RenderTreeFrame.Attribute(0, null, type.IsValueType ? Activator.CreateInstance(type) : null);
                nonUsedParameter.Setter(ref nullValueTreeFrame, target);
            }
        }

        private static IDictionary<string, WriteParameterAction> CreateParameterWriters(Type targetType)
        {
            var result = new Dictionary<string, WriteParameterAction>(StringComparer.OrdinalIgnoreCase);

            foreach (var propertyInfo in GetBindableProperties(targetType))
            {
                var propertySetter = MemberAssignment.CreatePropertySetter(targetType, propertyInfo);

                var propertyName = propertyInfo.Name;
                if (result.ContainsKey(propertyName))
                {
                    throw new InvalidOperationException(
                        $"The type '{targetType.FullName}' declares more than one parameter matching the " +
                        $"name '{propertyName.ToLowerInvariant()}'. Parameter names are case-insensitive and must be unique.");
                }

                result.Add(propertyName, new WriteParameterAction((ref RenderTreeFrame frame, object target) =>
                {
                    propertySetter.SetValue(target, frame.AttributeValue);
                },
                propertyInfo.PropertyType));
            }

            return result;
        }

        private static IEnumerable<PropertyInfo> GetBindableProperties(Type targetType)
            => MemberAssignment.GetPropertiesIncludingInherited(targetType, _bindablePropertyFlags)
                .Where(property => property.IsDefined(typeof(ParameterAttribute)));

        private static void ThrowForUnknownIncomingParameterName(Type targetType, string parameterName)
        {
            // We know we're going to throw by this stage, so it doesn't matter that the following
            // reflection code will be slow. We're just trying to help developers see what they did wrong.
            var propertyInfo = targetType.GetProperty(parameterName, _bindablePropertyFlags);
            if (propertyInfo != null)
            {
                if (!propertyInfo.IsDefined(typeof(ParameterAttribute)))
                {
                    throw new InvalidOperationException(
                        $"Object of type '{targetType.FullName}' has a property matching the name '{parameterName}', " +
                        $"but it does not have [{nameof(ParameterAttribute)}] applied.");
                }
                else
                {
                    // This should not happen
                    throw new InvalidOperationException(
                        $"No writer was cached for the property '{propertyInfo.Name}' on type '{targetType.FullName}'.");
                }
            }
            else
            {
                throw new InvalidOperationException(
                    $"Object of type '{targetType.FullName}' does not have a property " +
                    $"matching the name '{parameterName}'.");
            }
        }


        private class WriteParameterAction
        {
            public WriteParameterAction(WriteParameterActionSetter setter, Type propertyType)
            {
                Setter = setter;
                PropertyType = propertyType;
            }

            public delegate void WriteParameterActionSetter(ref RenderTreeFrame frame, object target);
            public WriteParameterActionSetter Setter { get; set; }

            public Type PropertyType { get; set; }
        }
    }
}
