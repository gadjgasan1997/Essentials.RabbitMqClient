using Essentials.RabbitMqClient.Context;
using Essentials.RabbitMqClient.Context.Implementations;
using Essentials.RabbitMqClient.Dictionaries;
using Essentials.RabbitMqClient.Metrics.Extensions;
using Essentials.RabbitMqClient.ModelBuilder.Extensions;
using Essentials.RabbitMqClient.OptionsProvider;
using Essentials.RabbitMqClient.OptionsProvider.Extensions;
using Essentials.RabbitMqClient.Publisher.Extensions;
using Essentials.RabbitMqClient.RabbitMqConnections.Extensions;
using Essentials.RabbitMqClient.Subscriber.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Essentials.Utils.Reflection.Helpers;
using Essentials.Serialization.Serializers;
using Essentials.Serialization.Deserializers;
using static Essentials.RabbitMqClient.Dictionaries.QueueLoggers;
using static Essentials.Serialization.EssentialsDeserializersFactory;
using static Essentials.Serialization.EssentialsSerializersFactory;
using static Essentials.RabbitMqClient.Helpers.ReflectionHelpers;
// ReSharper disable UnusedMethodReturnValue.Local

namespace Essentials.RabbitMqClient.Extensions;

/// <summary>
/// Методы расширения для <see cref="IServiceCollection" />
/// </summary>
public static class ServiceCollectionExtensions
{
    private static uint _isConfigured;

    /// <summary>
    /// Настраивает соединения с RabbitMq
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="configureAction">Действие по настройке билдера RabbitMq</param>
    /// <returns></returns>
    public static IServiceCollection ConfigureRabbitMqConnections(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<RabbitMqConfigurationBuilder>? configureAction = null)
    {
        if (Interlocked.Exchange(ref _isConfigured, 1) == 1)
            return services;
        
        try
        {
            var connectionsOptions = configuration.GetConnectionsOptions();
            var provider = services.ConfigureOptionsProvider(connectionsOptions);

            services.TryAddSingleton<IContextService, ContextService>();

            services
                .ConfigureRabbitMqConnectionFactory(connectionsOptions)
                .ConfigureRabbitMqModelBuilder()
                .ConfigureRabbitMqEventsPublisher()
                .ConfigureRabbitMqEventsSubscriber()
                .RegisterEventsHandlers(provider)
                .ConfigureMetricsService();
            
            AddByKey(KnownRabbitMqDeserializers.JSON, () => new NativeJsonDeserializer());
            AddByKey(KnownRabbitMqDeserializers.XML, () => new XmlDeserializer());
            AddByKey(KnownRabbitMqSerializers.JSON, () => new NativeJsonSerializer());
            AddByKey(KnownRabbitMqSerializers.XML, () => new XmlSerializer());

            var rabbitMqBuilder = new RabbitMqConfigurationBuilder()
                .AddLoggingMessageHandlerBehavior()
                .AddMetricsMessageHandlerBehavior()
                .AddLoggingMessagePublisherBehavior()
                .AddMetricsMessagePublisherBehavior();
            
            configureAction?.Invoke(rabbitMqBuilder);
            
            RabbitMqConfigurationBuilder.RegisterBehaviors(services);
   
            return services;
        }
        catch (Exception ex)
        {
            MainLogger.Error(ex, "Во время настройки подключений к RabbitMq произошла ошибка");
            return services;
        }
    }
    
    /// <summary>
    /// Регистрирует обработчики событий
    /// </summary>
    /// <param name="services"></param>
    /// <param name="provider"></param>
    /// <returns></returns>
    private static IServiceCollection RegisterEventsHandlers(
        this IServiceCollection services,
        IOptionsProvider provider)
    {
        var assemblies = AssemblyHelpers.GetAllAssembliesWithReferences().ToArray();
        
        foreach (var (_, connectionSubscriptions) in provider.GetSubscriptionsInfo())
        {
            foreach (var subscriptionInfo in connectionSubscriptions)
            {
                RegisterEventHandler(services, subscriptionInfo, assemblies);
            }
        }
        
        return services;
    }
}