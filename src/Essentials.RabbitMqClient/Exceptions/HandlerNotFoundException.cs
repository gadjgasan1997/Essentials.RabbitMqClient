using Essentials.RabbitMqClient.Subscriber.Models;

namespace Essentials.RabbitMqClient.Exceptions;

/// <summary>
/// Исключение о не найденном обработчике события
/// </summary>
public class HandlerNotFoundException : Exception
{
    internal HandlerNotFoundException(SubscriptionKey subscriptionKey)
        : base($"Для события с ключом '{subscriptionKey}' Не найден обработчик")
    { }
}