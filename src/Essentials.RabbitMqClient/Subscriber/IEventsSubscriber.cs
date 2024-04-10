using Essentials.RabbitMqClient.Subscriber.Models;

namespace Essentials.RabbitMqClient.Subscriber;

/// <summary>
/// Сервис для подписки на события
/// </summary>
internal interface IEventsSubscriber
{
    /// <summary>
    /// Подписывается на событие
    /// </summary>
    /// <param name="subscriptionParams">Опции подписки на событие</param>
    /// <typeparam name="TEvent">Тип события</typeparam>
    /// <returns></returns>
    void Subscribe<TEvent>(SubscriptionParams subscriptionParams) where TEvent : IEvent;
}