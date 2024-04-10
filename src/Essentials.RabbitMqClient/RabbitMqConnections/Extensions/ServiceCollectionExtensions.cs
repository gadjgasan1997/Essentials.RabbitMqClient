using Essentials.RabbitMqClient.Options;
using Essentials.RabbitMqClient.RabbitMqConnections.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Essentials.RabbitMqClient.RabbitMqConnections.Extensions;

/// <summary>
/// Методы расширения для <see cref="IServiceCollection" />
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Настраивает фабрику создания соединений RabbitMq
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionsOptions">Опции соединений</param>
    /// <returns>Фабрика</returns>
    public static IServiceCollection ConfigureRabbitMqConnectionFactory(
        this IServiceCollection services,
        ConnectionsOptions connectionsOptions)
    {
        var factory = new RabbitMqConnectionFactory();

        foreach (var connectionOptions in connectionsOptions.Connections)
            factory.AddConnection(connectionOptions);
        
        services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>(_ => factory);
        services.AddSingleton<IChannelFactory, ChannelFactory>();
        
        return services;
    }
}