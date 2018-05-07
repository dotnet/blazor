using System;
using System.Net.Http;
using Microsoft.AspNetCore.Blazor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Blazor.Server.Rendering
{
    /// <summary>
    /// 
    /// </summary>
    public class PreServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider _underlyingProvider;

        /// <summary>
        /// Constructs an instance of <see cref="PreServiceProvider"/>.
        /// </summary>
        public PreServiceProvider() : this(null)
        {
        }

        /// <summary>
        /// Constructs an instance of <see cref="PreServiceProvider"/>.
        /// </summary>
        /// <param name="configure">A callback that can be used to configure the <see cref="IServiceCollection"/>.</param>
        public PreServiceProvider(Action<IServiceCollection> configure)
        {
            var serviceCollection = new ServiceCollection();
            AddDefaultServices(serviceCollection);
            configure?.Invoke(serviceCollection);
            _underlyingProvider = serviceCollection.BuildServiceProvider();
        }

        /// <inheritdoc />
        public object GetService(Type serviceType)
            => _underlyingProvider.GetService(serviceType);

        private void AddDefaultServices(ServiceCollection serviceCollection)
        {
            var uriHelper = new PreUriHelper();
            serviceCollection.AddSingleton<IUriHelper>(uriHelper);
            serviceCollection.AddSingleton(new HttpClient(/*new BrowserHttpMessageHandler()*/)
            {
                BaseAddress = new Uri(uriHelper.GetBaseUriPrefix())
            });
        }
    }
}