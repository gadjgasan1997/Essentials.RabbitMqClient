namespace Essentials.RabbitMqClient.Subscriber.MessageProcessing;

/// <summary>
/// Делегат обработки сообщения
/// </summary>
public delegate Task MessageHandlerDelegate();

/// <summary>
/// Перехватчик обработки сообщения
/// </summary>
public interface IMessageHandlerBehavior
{
    /// <summary>
    /// Обрабатывает сообщение.
    /// Вызывается перед основным (конечным) обработчиком.
    /// </summary>
    /// <param name="next">Делегат вызова следующего перехватчика/конечного обработчика</param>
    /// <returns></returns>
    Task Handle(MessageHandlerDelegate next);
}