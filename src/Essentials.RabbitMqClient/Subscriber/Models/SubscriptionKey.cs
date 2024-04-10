using System.Text;
using Essentials.RabbitMqClient.Models;
using Essentials.Utils.Extensions;

namespace Essentials.RabbitMqClient.Subscriber.Models;

/// <summary>
/// Ключ подписки на событие
/// </summary>
public readonly record struct SubscriptionKey
{
    private SubscriptionKey(QueueKey queueKey, RoutingKey routingKey)
    {
        QueueKey = queueKey.CheckNotNull();
        RoutingKey = routingKey.CheckNotNull();
    }
    
    /// <summary>
    /// Ключ очереди
    /// </summary>
    public QueueKey QueueKey { get; }

    /// <summary>
    /// Ключ маршрутизации
    /// </summary>
    public RoutingKey RoutingKey { get; }

    /// <summary>
    /// Создает ключ подписки на событие
    /// </summary>
    /// <param name="queueKey">Ключ очереди</param>
    /// <param name="routingKey">Ключ маршрутизации</param>
    /// <returns>Ключ</returns>
    internal static SubscriptionKey New(QueueKey queueKey, RoutingKey routingKey) =>
        new(queueKey, routingKey);

    /// <summary>
    /// Возвращает строковое представление ключа
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var builder = new StringBuilder();

        builder
            .Append("{ ")
            .Append("\"Ключ очереди\": ")
            .Append(QueueKey)
            .Append(", ")
            .Append("\"Ключ маршрутизации\": \"")
            .Append(RoutingKey)
            .Append("\" }");
        
        return builder.ToString();
    }
}