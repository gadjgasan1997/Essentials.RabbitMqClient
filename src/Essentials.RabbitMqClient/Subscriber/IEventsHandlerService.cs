using Essentials.RabbitMqClient.Models;
using Essentials.RabbitMqClient.Subscriber.MessageProcessing;
using Essentials.RabbitMqClient.Subscriber.Models;
using RabbitMQ.Client.Events;

namespace Essentials.RabbitMqClient.Subscriber;

/// <summary>
/// Информация об обработчике события
/// </summary>
/// <param name="Handler">Обработчик</param>
internal record HandlerInfo(Func<BasicDeliverEventArgs, Task> Handler);

/// <summary>
/// Сервис для обработки событий
/// </summary>
internal interface IEventsHandlerService
{
    /// <summary>
    /// Пытается вернуть обработчик для события
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <param name="subscriptionKey">Ключ события</param>
    /// <param name="handlerInfo">Информация об обработчике события</param>
    /// <returns></returns>
    bool TryGetHandler(
        ConnectionKey connectionKey,
        SubscriptionKey subscriptionKey,
        out HandlerInfo? handlerInfo);

    /// <summary>
    /// Регистрирует обработчик события
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <param name="subscriptionKey">Ключ события</param>
    /// <param name="options">Опции подписки</param>
    /// <typeparam name="TEvent">Тип события</typeparam>
    void RegisterEventHandler<TEvent>(
        ConnectionKey connectionKey,
        SubscriptionKey subscriptionKey,
        SubscriptionOptions options)
        where TEvent : IEvent;

    /// <summary>
    /// Пытается обработать событие получения сообщения
    /// </summary>
    /// <param name="options">Опции подписки</param>
    /// <returns></returns>
    Task HandleEvent(SubscriptionOptions options);
}