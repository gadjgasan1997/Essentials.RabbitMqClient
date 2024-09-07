using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Essentials.Utils.Reflection.Helpers;
using Essentials.Serialization.Serializers;
using Essentials.Serialization.Deserializers;
using Essentials.RabbitMqClient.Context;
using Essentials.RabbitMqClient.Context.Implementations;
using Essentials.RabbitMqClient.Dictionaries;
using Essentials.RabbitMqClient.Metrics.Extensions;
using Essentials.RabbitMqClient.RabbitMqModelBuilder.Extensions;
using Essentials.RabbitMqClient.Publisher.Extensions;
using Essentials.RabbitMqClient.RabbitMqConnections.Extensions;
using Essentials.RabbitMqClient.Subscriber.Extensions;
using static Essentials.RabbitMqClient.Dictionaries.QueueLoggers;
using static Essentials.Serialization.EssentialsDeserializersFactory;
using static Essentials.Serialization.EssentialsSerializersFactory;
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
            var assemblies = AssemblyHelpers.GetAllAssembliesWithReferences().ToArray();
            
            var connectionsOptions = configuration.GetConnectionsOptions();
            var connectionsToConfigure = connectionsOptions.Connections
                .Where(connectionOptions => connectionOptions.Model is not null)
                .ToDictionary(
                    connectionOptions => connectionOptions.Name,
                    connectionOptions => connectionOptions.Model!);
            
            services
                .AddSingleton<IContextService, ContextService>()
                .ConfigureRabbitMqModelBuilder(connectionsToConfigure)
                .ConfigureRabbitMqConnectionFactory(connectionsOptions)
                .ConfigureRabbitMqEventsPublisher(connectionsToConfigure, assemblies)
                .ConfigureRabbitMqEventsSubscriber(connectionsToConfigure, assemblies)
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
}