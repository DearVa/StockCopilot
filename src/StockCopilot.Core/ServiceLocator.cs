using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using StockCopilot.Abstractions.Extensions;

namespace StockCopilot.Core;

public static class ServiceLocator
{
    public static IServiceCollection ServiceCollection { get; } = new ServiceCollection();

    private static IServiceProvider ServiceProvider => serviceProvider.NotNull("ServiceLocator has not been built.");

    private static IServiceProvider? serviceProvider;

    public static void Build()
    {
        serviceProvider = ServiceCollection.BuildServiceProvider();
    }

    public static bool TryResolve(Type serviceType, [NotNullWhen(true)] out object? service)
    {
        service = ServiceProvider.GetService(serviceType);
        return service is not null;
    }

    public static object Resolve(Type serviceType)
    {
        return ServiceProvider.GetRequiredService(serviceType);
    }

    public static TService Resolve<TService>() where TService : class
    {
        return ServiceProvider.GetRequiredService<TService>();
    }
}