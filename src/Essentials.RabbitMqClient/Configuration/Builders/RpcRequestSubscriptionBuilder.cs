using Essentials.RabbitMqClient.Options;
using Essentials.RabbitMqClient.Configuration.Builders.Abstractions;

namespace Essentials.RabbitMqClient.Configuration.Builders;

/// <summary>
/// Билдер подписки на событие Rpc запроса
/// </summary>
/// <typeparam name="TEvent">Тип события</typeparam>
/// <typeparam name="THandler">Тип обработчика</typeparam>
public class RpcRequestSubscriptionBuilder<TEvent, THandler> : BaseSubscriptionBuilder<TEvent>
    where TEvent : IEvent
    where THandler : IEventHandler<TEvent>
{
    internal RpcRequestSubscriptionBuilder(string queueName, string routingKey)
        : base(queueName, routingKey)
    { }
    
    /// <summary>
    /// Билдит опции подписки на событие
    /// </summary>
    /// <returns>Опции подписки на событие</returns>
    internal new SubscriptionOptions Build()
    {
        var options = base.Build();
        options.Options.TypeName = typeof(TEvent).FullName!;
        options.Options.HandlerType = typeof(THandler);
        return options;
    }
}