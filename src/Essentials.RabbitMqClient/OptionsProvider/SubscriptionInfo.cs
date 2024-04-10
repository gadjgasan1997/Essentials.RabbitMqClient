using Essentials.RabbitMqClient.Models;

namespace Essentials.RabbitMqClient.OptionsProvider;

/// <summary>
/// Информация по подписке на событие
/// </summary>
/// <param name="QueueName">Название очереди</param>
/// <param name="RoutingKey">Ключ маршрутизации</param>
/// <param name="EventTypeName">Название типа события</param>
/// <param name="HandlerTypeName">Название типа обработчика события</param>
internal record SubscriptionInfo(
    QueueKey QueueName,
    RoutingKey RoutingKey,
    string EventTypeName,
    string? HandlerTypeName);