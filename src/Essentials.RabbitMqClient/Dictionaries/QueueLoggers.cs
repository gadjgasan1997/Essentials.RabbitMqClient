using NLog;
using Essentials.RabbitMqClient.Publisher;
using Essentials.RabbitMqClient.Subscriber;

namespace Essentials.RabbitMqClient.Dictionaries;

/// <summary>
/// Логгеры для взаиодействия с очередью
/// </summary>
public static class QueueLoggers
{
    /// <summary>
    /// Основной логгер
    /// </summary>
    public static Logger MainLogger { get; } = LogManager.GetLogger("Essentials.RabbtMq.Client.Main");
    
    /// <summary>
    /// Логгер для сервиса <see cref="IEventsPublisher" />
    /// </summary>
    public static Logger EventsPublisherLogger { get; } = LogManager.GetLogger("Essentials.RabbtMq.Client.EventsPublisher");
    
    /// <summary>
    /// Логгер для сервиса <see cref="IEventsSubscriber" />
    /// </summary>
    public static Logger EventsSubscriberLogger { get; } = LogManager.GetLogger("Essentials.RabbtMq.Client.EventsSubscriber");
}