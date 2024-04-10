namespace Essentials.RabbitMqClient;

/// <summary>
/// Обработчик получаемого из очереди сообщения
/// </summary>
/// <typeparam name="TEvent">Тип события</typeparam>
public interface IEventHandler<in TEvent>
    where TEvent : IEvent
{
    /// <summary>
    /// Обрабатывает событие
    /// </summary>
    /// <param name="event">Событие</param>
    /// <returns></returns>
    Task HandleAsync(TEvent @event);
}