using Microsoft.Extensions.DependencyInjection;
using StockCopilot.Abstractions.Extensions;

namespace StockCopilot.Core;

public static class ServiceLocator
{
    public static IServiceCollection ServiceCollection { get; } = new ServiceCollection();

    private static IServiceProvider ServiceProvider => serviceProvider.NotNull("ServiceLocator has not been built.");

    private static IServiceProvider? serviceProvider;

    public static IServiceCollection AddSingleton<TService>(TService service) where TService : class
    {
        return ServiceCollection.AddSingleton(service);
    }

    public static IServiceCollection AddSingleton<TService, TImplementation>() where TService : class where TImplementation : class, TService
    {
        return ServiceCollection.AddSingleton<TService, TImplementation>();
    }

    public static IServiceCollection AddSingleton<TService>(Func<IServiceProvider, TService> implementationFactory) where TService : class
    {
        return ServiceCollection.AddSingleton(implementationFactory);
    }

    public static void Build()
    {
        serviceProvider = ServiceCollection.BuildServiceProvider();
    }

    public static TService Resolve<TService>() where TService : class
    {
        return ServiceProvider.GetRequiredService<TService>();
    }
}