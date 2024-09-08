namespace Essentials.RabbitMqClient.Configuration.Builders;

/// <summary>
/// Билдер публикации события в ответ на Rpc запрос
/// </summary>
/// <typeparam name="TEvent">Тип события</typeparam>
public class PublishRpcResponseBuilder<TEvent> : PublishBuilder<TEvent>
    where TEvent : IEvent
{
    internal PublishRpcResponseBuilder(string exchangeName)
        : base(exchangeName)
    { }
}