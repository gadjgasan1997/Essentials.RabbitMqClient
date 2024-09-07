using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Essentials.RabbitMqClient.Options;
using Essentials.RabbitMqClient.Subscriber.Helpers;
using Essentials.RabbitMqClient.Subscriber.Implementations;

namespace Essentials.RabbitMqClient.Subscriber.Extensions;

/// <summary>
/// Методы расширения для <see cref="IServiceCollection" />
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Настраивает сервис для подписки на события
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionsOptions"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureRabbitMqEventsSubscriber(
        this IServiceCollection services,
        IReadOnlyDictionary<string, ModelOptions> connectionsOptions,
        Assembly[] assemblies)
    {
        var provider = OptionsProviderBuilder.Build(connectionsOptions, assemblies);
        
        return services
            .RegisterEventsHandlers(provider, assemblies)
            .AddSingleton<IOptionsProvider, OptionsProvider>(_ => provider)
            .AddSingleton<IEventsSubscriber, EventsSubscriber>()
            .AddSingleton<IEventsHandlerService, EventsHandlerService>()
            .AddHostedService<HostedService>();
    }
    
    /// <summary>
    /// Регистрирует обработчики событий
    /// </summary>
    /// <param name="services"></param>
    /// <param name="provider"></param>
    /// <param name="assemblies">Сборки</param>
    /// <returns></returns>
    private static IServiceCollection RegisterEventsHandlers(
        this IServiceCollection services,
        OptionsProvider provider,
        Assembly[] assemblies)
    {
        foreach (var options in provider.GetOptionsForRegisterEventsHandlers())
            ReflectionHelpers.RegisterEventHandler(services, options, assemblies);
        
        return services;
    }
}