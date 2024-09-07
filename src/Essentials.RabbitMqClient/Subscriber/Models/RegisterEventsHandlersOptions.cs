namespace Essentials.RabbitMqClient.Subscriber.Models;

/// <summary>
/// Опции регистрации обработчиков
/// </summary>
/// <param name="EventTypeName">Название типа события</param>
/// <param name="HandlerTypeName">Название типа обработчика события</param>
internal record RegisterEventsHandlersOptions(
    string EventTypeName,
    string? HandlerTypeName);