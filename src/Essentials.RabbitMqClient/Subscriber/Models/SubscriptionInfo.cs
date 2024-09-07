using Essentials.RabbitMqClient.Models;

namespace Essentials.RabbitMqClient.Subscriber.Models;

/// <summary>
/// Информация по подписке на событие
/// </summary>
/// <param name="QueueName">Название очереди</param>
/// <param name="RoutingKey">Ключ маршрутизации</param>
/// <param name="EventTypeName">Название типа события</param>
internal record SubscriptionInfo(
    QueueKey QueueName,
    RoutingKey RoutingKey,
    string EventTypeName);