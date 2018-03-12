using Microsoft.Extensions.DependencyInjection;

namespace PrerenderingApp.Client
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSharedServices(this IServiceCollection configure)
        {
            configure.AddSingleton<Greeter>();
        }
    }
}