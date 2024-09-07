using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Essentials.RabbitMqClient.Options;
using Essentials.RabbitMqClient.Publisher.Implementations;

namespace Essentials.RabbitMqClient.Publisher.Extensions;

/// <summary>
/// Методы расширения для <see cref="IServiceCollection" />
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Настраивает сервис для публикации событий
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionsOptions"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureRabbitMqEventsPublisher(
        this IServiceCollection services,
        IReadOnlyDictionary<string, ModelOptions> connectionsOptions,
        Assembly[] assemblies)
    {
        var provider = OptionsProviderBuilder.Build(connectionsOptions, assemblies);
        
        return 
            services
                .AddSingleton<IOptionsProvider, OptionsProvider>(_ => provider)
                .AddSingleton<IAskManager, AskManager>()
                .AddSingleton<IEventsPublisher, EventsPublisher>();
    }
}