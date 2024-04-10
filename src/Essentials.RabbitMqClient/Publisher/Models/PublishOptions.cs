using Essentials.Utils.Extensions;

namespace Essentials.RabbitMqClient.Publisher.Models;

/// <summary>
/// Опции публикации
/// </summary>
/// <param name="ContentType">Тип содержимого сообщение</param>
/// <param name="RetryCount">Количество попыток</param>
/// <param name="DeliveryMode">Режим доставки сообщения</param>
/// <param name="Behaviors">Список перехватчиков сообщения</param>
internal record PublishOptions(
    string ContentType,
    int RetryCount,
    byte DeliveryMode,
    IEnumerable<Type> Behaviors)
{
    /// <summary>
    /// Тип содержимого сообщения
    /// </summary>
    public string ContentType { get; } = ContentType.CheckNotNullOrEmpty();

    /// <summary>
    /// Количество попыток
    /// </summary>
    public int RetryCount { get; } = RetryCount;

    /// <summary>
    /// Режим доставки сообщения
    /// </summary>
    public byte DeliveryMode { get; } = DeliveryMode;

    /// <summary>
    /// Список перехватчиков сообщения
    /// </summary>
    public IEnumerable<Type> Behaviors { get; } = Behaviors;
}