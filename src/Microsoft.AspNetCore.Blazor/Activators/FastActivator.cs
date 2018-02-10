// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;

namespace Microsoft.AspNetCore.Blazor.Activators
{
	public class FastActivator
	{
		private static ConcurrentDictionary<Type, Lazy<Func<object>>> factoryCache = new ConcurrentDictionary<Type, Lazy<Func<object>>>();

		public static T CreateInstance<T>() where T : new()
		{
			return FastActivatorImpl<T>.Create();
		}

		public static object CreateInstance(Type type)
		{
			var lazy = factoryCache.GetOrAdd(type,
				k => new Lazy<Func<object>>(() => DynamicModuleLambdaCompiler.GenerateFactory(k)));
			return lazy.Value();
		}

		private static class FastActivatorImpl<T> where T : new()
		{
			public static readonly Func<T> Create =
				DynamicModuleLambdaCompiler.GenerateFactory<T>();
		}
	}
}