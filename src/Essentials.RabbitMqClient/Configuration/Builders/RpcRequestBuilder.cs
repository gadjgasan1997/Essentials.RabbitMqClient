using Essentials.Utils.Extensions;
using Essentials.RabbitMqClient.Options;

namespace Essentials.RabbitMqClient.Configuration.Builders;

/// <summary>
/// Билдер Rpc запроса
/// </summary>
/// <typeparam name="TEvent">Тип события</typeparam>
public class RpcRequestBuilder<TEvent> : PublishBuilder<TEvent>
    where TEvent : IEvent
{
    private readonly string _replyTo;
    private int? _timeout;
    
    internal RpcRequestBuilder(string exchangeName, string routingKey, string replyTo)
        : base(exchangeName, routingKey)
    {
        routingKey.CheckNotNullOrEmpty("Ключ маршрутизации для конфигурации не может быть пустым");

        _replyTo = replyTo.CheckNotNullOrEmpty(
            "Ключ маршрутизации, с которым требуется ответить на запрос не может быть пустым");
    }
    
    /// <summary>
    /// Устанавливает таймаут ожидания ответа на запрос в секундах
    /// </summary>
    /// <param name="timeout">Таймаут</param>
    /// <returns>Билдер Rpc запроса</returns>
    public RpcRequestBuilder<TEvent> SetTimout(int timeout)
    {
        _timeout = timeout;
        return this;
    }
    
    /// <summary>
    /// Билдит опции Rpc запроса
    /// </summary>
    /// <returns>Опции Rpc запроса</returns>
    internal new RpcRequestOptions Build()
    {
        return new RpcRequestOptions
        {
            Key = new RpcRequestOptions.KeyElement
            {
                Exchange = ExchangeName,
                RoutingKey = RoutingKey!,
                TypeName = typeof(TEvent).FullName!
            },
            Options = new RpcRequestOptions.ValueElement
            {
                ContentType = ContentType,
                RetryCount = RetryCount,
                DeliveryMode = DeliveryMode,
                ReplyTo = _replyTo,
                Timeout = _timeout,
                Behaviors = Behaviors
            }
        };
    }
}