using Essentials.RabbitMqClient.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Essentials.RabbitMqClient.OptionsProvider.Extensions;

/// <summary>
/// Методы расширения для <see cref="IServiceCollection" />
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Настраивает провайдер для получения опций
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionsOptions"></param>
    /// <returns></returns>
    public static IOptionsProvider ConfigureOptionsProvider(
        this IServiceCollection services,
        ConnectionsOptions connectionsOptions)
    {
        var provider = new Implementations.OptionsProvider();
        
        foreach (var connectionOptions in connectionsOptions.Connections)
            new Builder().AddConnectionOptionsToProvider(provider, connectionOptions);

        services.AddSingleton<IOptionsProvider, Implementations.OptionsProvider>(_ => provider);
        return provider;
    }
}