using Microsoft.Extensions.DependencyInjection;

namespace N1coLoyalty.Application.FunctionalTests.ExtensionMethods;

internal static class ExtensionMethods
{
    public static void SwapTransient<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory)
    {
        if (services.Any(x => x.ServiceType == typeof(TService) && x.Lifetime == ServiceLifetime.Transient))
        {
            var serviceDescriptors = services.Where(x => x.ServiceType == typeof(TService) && x.Lifetime == ServiceLifetime.Transient).ToList();
            foreach (var serviceDescriptor in serviceDescriptors)
            {
                services.Remove(serviceDescriptor);
            }
        }

        services.AddTransient(typeof(TService), (sp) => implementationFactory(sp) ?? throw new InvalidOperationException("Implementation factory returned null."));
    }
}