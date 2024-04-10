using Essentials.RabbitMqClient.Models;

namespace Essentials.RabbitMqClient.Publisher.Models;

/// <summary>
/// Опции Rpc запроса
/// </summary>
/// <param name="ContentType">Тип содержимого сообщение</param>
/// <param name="RetryCount">Количество попыток</param>
/// <param name="DeliveryMode">Режим доставки сообщения</param>
/// <param name="ReplyTo">Ключ маршрутизации для ответов</param>
/// <param name="Timeout">Время ожидания ответов в секундах</param>
/// <param name="Behaviors">Список перехватчиков сообщения</param>
internal record RpcRequestOptions(
    string ContentType,
    int RetryCount,
    byte DeliveryMode,
    RoutingKey ReplyTo,
    TimeSpan Timeout,
    IEnumerable<Type> Behaviors)
    : PublishOptions(ContentType, RetryCount, DeliveryMode, Behaviors)
{
    /// <summary>
    /// Очередь для ответов
    /// </summary>
    public RoutingKey ReplyTo { get; } = ReplyTo;

    /// <summary>
    /// Время ожидания ответов из <see cref="ReplyTo"/> в секундах
    /// </summary>
    public TimeSpan Timeout { get; } = Timeout;
}