using Essentials.RabbitMqClient.Models;

namespace Essentials.RabbitMqClient.Subscriber.Models;

/// <summary>
/// Опции подписки на событие
/// </summary>
/// <param name="ConnectionKey">Ключ соединения</param>
/// <param name="QueueKey">Ключ очереди</param>
/// <param name="RoutingKey">Ключ маршрутизации</param>
internal record SubscriptionParams(
    ConnectionKey ConnectionKey,
    QueueKey QueueKey,
    RoutingKey RoutingKey);