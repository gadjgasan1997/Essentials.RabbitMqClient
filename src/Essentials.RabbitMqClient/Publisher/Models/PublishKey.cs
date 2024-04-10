using System.Text;
using Essentials.Utils.Extensions;
using Essentials.RabbitMqClient.Models;

namespace Essentials.RabbitMqClient.Publisher.Models;

/// <summary>
/// Ключ публикации события
/// </summary>
public readonly record struct PublishKey
{
    private PublishKey(string exchange, EventKey eventKey, RoutingKey? routingKey = null)
    {
        Exchange = exchange.CheckNotNullOrEmpty();
        EventKey = eventKey.CheckNotNull();
        RoutingKey = routingKey;
    }
    
    /// <summary>
    /// Обменник
    /// </summary>
    public string Exchange { get; }
    
    /// <summary>
    /// Ключ события
    /// </summary>
    public EventKey EventKey { get; }

    /// <summary>
    /// Ключ маршрутизации
    /// </summary>
    public RoutingKey? RoutingKey { get; }

    /// <summary>
    /// Создает ключ публикации события
    /// </summary>
    /// <param name="exchange">Обменник</param>
    /// <param name="eventKey">Ключ события</param>
    /// <returns>Ключ</returns>
    internal static PublishKey New(string exchange, EventKey eventKey) =>
        new(exchange, eventKey);
    
    /// <summary>
    /// Создает ключ публикации события
    /// </summary>
    /// <param name="exchange">Обменник</param>
    /// <param name="eventKey">Ключ события</param>
    /// <param name="routingKey">Ключ маршрутизации</param>
    /// <returns>Ключ</returns>
    internal static PublishKey New(string exchange, EventKey eventKey, RoutingKey routingKey) =>
        new(exchange, eventKey, routingKey);
    
    /// <summary>
    /// Возвращает строковое представление ключа
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var builder = new StringBuilder();

        builder
            .Append("{ ")
            .Append("\"Обменник\": ")
            .Append(Exchange)
            .Append(", ")
            .Append("\"Ключ события\": ")
            .Append(EventKey)
            .Append(", ")
            .Append("\"Ключ маршрутизации\": \"")
            .Append(RoutingKey)
            .Append("\" }");
        
        return builder.ToString();
    }
}