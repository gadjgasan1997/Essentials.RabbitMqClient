namespace Essentials.RabbitMqClient.Publisher.MessageProcessing;

/// <summary>
/// Делегат публикации сообщения
/// </summary>
public delegate Task MessagePublishDelegate();

/// <summary>
/// Перехватчик публикации сообщения
/// </summary>
public interface IMessagePublishBehavior
{
    /// <summary>
    /// Обрабатывает сообщение.
    /// Вызывается перед публикацией сообщения.
    /// </summary>
    /// <param name="next">Делегат вызова следующего перехватчика/публикации сообщения</param>
    /// <returns></returns>
    Task Handle(MessagePublishDelegate next);
}