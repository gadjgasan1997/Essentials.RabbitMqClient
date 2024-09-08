using Essentials.RabbitMqClient.Options;
using Essentials.RabbitMqClient.Configuration.Builders.Abstractions;

namespace Essentials.RabbitMqClient.Configuration.Builders;

/// <summary>
/// Билдер подписки на событие ответа на Rpc запрос
/// </summary>
/// <typeparam name="TEvent">Тип события</typeparam>
public class RpcResponseSubscriptionBuilder<TEvent> : BaseSubscriptionBuilder<TEvent>
    where TEvent : IEvent
{
    internal RpcResponseSubscriptionBuilder(string queueName, string routingKey)
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
        options.Options.Correlation = true;
        return options;
    }
}