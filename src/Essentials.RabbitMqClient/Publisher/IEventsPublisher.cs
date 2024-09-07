using LanguageExt.Common;
using Essentials.RabbitMqClient.Publisher.Models;

namespace Essentials.RabbitMqClient.Publisher;

/// <summary>
/// Сервис для публикации событий
/// </summary>
public interface IEventsPublisher
{
    /// <summary>
    /// Публикует сообщение в очередь
    /// </summary>
    /// <param name="event">Событие</param>
    /// <typeparam name="TEvent">Тип события</typeparam>
    void Publish<TEvent>(TEvent @event) where TEvent : IEvent;

    /// <summary>
    /// Публикует сообщение в очередь
    /// </summary>
    /// <param name="event">Событие</param>
    /// <param name="publishParams">Параметры публикации сообщения</param>
    /// <typeparam name="TEvent">Тип события</typeparam>
    void Publish<TEvent>(TEvent @event, PublishParams publishParams) where TEvent : IEvent;

    /// <summary>
    /// Публикует ответ на Rpc вызов в очередь
    /// </summary>
    /// <param name="event">Событие</param>
    /// <typeparam name="TEvent">Тип события</typeparam>
    void PublishRpcResponse<TEvent>(TEvent @event) where TEvent : IEvent;

    /// <summary>
    /// Публикует сообщение в очередь и ждет асинхронно ответ
    /// </summary>
    /// <remarks>Timeout задается через <see cref="Options.PublishOptions.Options"/></remarks>
    /// <param name="event">Событие</param>
    /// <param name="token">Токен отмены</param>
    /// <typeparam name="TEvent">Тип события</typeparam>
    /// <typeparam name="TAnswer">Тип входящего события</typeparam>
    Task<Result<TAnswer>> AskAsync<TEvent, TAnswer>(TEvent @event, CancellationToken token)
        where TEvent : IEvent
        where TAnswer : IEvent;

    /// <summary>
    /// Публикует сообщение в очередь и ждет асинхронно ответ
    /// </summary>
    /// <remarks>Timeout задается через <see cref="Options.PublishOptions.Options"/></remarks>
    /// <param name="event">Событие</param>
    /// <param name="publishParams">Параметры публикации сообщения</param>
    /// <param name="token">Токен отмены</param>
    /// <typeparam name="TEvent">Тип события</typeparam>
    /// <typeparam name="TAnswer">Тип входящего события</typeparam>
    Task<Result<TAnswer>> AskAsync<TEvent, TAnswer>(
        TEvent @event,
        PublishParams publishParams,
        CancellationToken token)
        where TEvent : IEvent where TAnswer : IEvent;
}