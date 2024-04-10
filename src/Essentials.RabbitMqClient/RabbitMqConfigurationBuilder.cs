using Essentials.RabbitMqClient.Publisher.MessageProcessing;
using Essentials.RabbitMqClient.Publisher.MessageProcessing.Behaviors;
using Essentials.RabbitMqClient.Subscriber.MessageProcessing;
using Essentials.RabbitMqClient.Subscriber.MessageProcessing.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using static Microsoft.Extensions.DependencyInjection.ServiceLifetime;

// ReSharper disable MemberCanBePrivate.Global

namespace Essentials.RabbitMqClient;

/// <summary>
/// Билдер конфигурации RabbitMq
/// </summary>
public class RabbitMqConfigurationBuilder
{
    private static readonly List<ServiceDescriptor> _messageHandlerBehaviors = new();
    
    private static readonly List<ServiceDescriptor> _messagePublisherBehaviors = new();
    
    internal RabbitMqConfigurationBuilder()
    { }

    /// <summary>
    /// Добавляет перехватчик обработки сообщения
    /// </summary>
    /// <param name="lifetime">Время жизни</param>
    /// <typeparam name="TBehavior">Перехватчик</typeparam>
    /// <returns>Билдер</returns>
    public RabbitMqConfigurationBuilder AddMessageHandlerBehavior<TBehavior>(ServiceLifetime? lifetime = Singleton)
        where TBehavior : IMessageHandlerBehavior
    {
        var behaviorLifetime = lifetime ?? Singleton;
        
        var implementationType = typeof(TBehavior);
        var descriptor = new ServiceDescriptor(typeof(IMessageHandlerBehavior), implementationType, behaviorLifetime);
        
        _messageHandlerBehaviors.Add(descriptor);
        return this;
    }

    /// <summary>
    /// Добавляет перехватчик обработки сообщения для сбора логов
    /// </summary>
    /// <returns>Билдер</returns>
    internal RabbitMqConfigurationBuilder AddLoggingMessageHandlerBehavior() =>
        AddMessageHandlerBehavior<LoggingMessageHandlerBehavior>();

    /// <summary>
    /// Добавляет перехватчик обработки сообщения для сбора метрик
    /// </summary>
    /// <returns>Билдер</returns>
    internal RabbitMqConfigurationBuilder AddMetricsMessageHandlerBehavior() =>
        AddMessageHandlerBehavior<MetricsMessageHandlerBehavior>();

    /// <summary>
    /// Добавляет перехватчик отправки сообщения
    /// </summary>
    /// <param name="lifetime">Время жизни</param>
    /// <typeparam name="TBehavior">Перехватчик</typeparam>
    /// <returns>Билдер</returns>
    public RabbitMqConfigurationBuilder AddMessagePublisherBehavior<TBehavior>(ServiceLifetime? lifetime = Singleton)
        where TBehavior : IMessagePublishBehavior
    {
        var behaviorLifetime = lifetime ?? Singleton;
        
        var implementationType = typeof(TBehavior);
        var descriptor = new ServiceDescriptor(typeof(IMessagePublishBehavior), implementationType, behaviorLifetime);
        
        _messagePublisherBehaviors.Add(descriptor);
        return this;
    }

    /// <summary>
    /// Добавляет перехватчик отправки сообщения для сбора логов
    /// </summary>
    /// <returns>Билдер</returns>
    internal RabbitMqConfigurationBuilder AddLoggingMessagePublisherBehavior() =>
        AddMessagePublisherBehavior<LoggingMessagePublisherBehavior>();

    /// <summary>
    /// Добавляет перехватчик отправки сообщения для сбора метрик
    /// </summary>
    /// <returns>Билдер</returns>
    internal RabbitMqConfigurationBuilder AddMetricsMessagePublisherBehavior() =>
        AddMessagePublisherBehavior<MetricsMessagePublisherBehavior>();

    /// <summary>
    /// Регистрирует все перехватчики сообщений
    /// </summary>
    /// <param name="services"></param>
    internal static void RegisterBehaviors(IServiceCollection services)
    {
        foreach (var serviceDescriptor in _messageHandlerBehaviors)
            services.TryAddEnumerable(serviceDescriptor);
        
        foreach (var serviceDescriptor in _messagePublisherBehaviors)
            services.TryAddEnumerable(serviceDescriptor);
    }
}