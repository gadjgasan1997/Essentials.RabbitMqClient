using Essentials.Utils.Extensions;

namespace Essentials.RabbitMqClient.Subscriber.Models;

/// <summary>
/// Опции подписки на очередь
/// </summary>
/// <param name="EventTypeName">Название типа события</param>
/// <param name="HandlerTypeName">Тип обработчика сообщения</param>
/// <param name="ContentType">Тип содержимого сообщения</param>
/// <param name="PrefetchCount">Количество сообщений, которое может забрать подписчик</param>
/// <param name="Correlation">Признак необходимости установить событие в задаче (RPC)</param>
/// <param name="Behaviors">Список перехватчиков сообщения</param>
internal record SubscriptionOptions(
    string EventTypeName,
    string? HandlerTypeName,
    string ContentType,
    ushort PrefetchCount,
    bool Correlation,
    List<Type> Behaviors)
{
    /// <summary>
    /// Название типа события
    /// </summary>
    public string EventTypeName { get; } = EventTypeName.CheckNotNullOrEmpty();
    
    /// <summary>
    /// Название типа обработчика события
    /// </summary>
    public string? HandlerTypeName { get; } = HandlerTypeName?.FullTrim();
    
    /// <summary>
    /// Тип содержимого сообщения
    /// </summary>
    public string ContentType { get; } = ContentType.CheckNotNullOrEmpty();

    /// <summary>
    /// Количество сообщений, которое может забрать подписчик
    /// </summary>
    public ushort PrefetchCount { get; } = PrefetchCount;

    /// <summary>
    /// Признак необходимости установить событие в задаче (RPC)
    /// </summary>
    public bool Correlation { get; } = Correlation;
    
    /// <summary>
    /// Список перехватчиков сообщения
    /// </summary>
    public List<Type> Behaviors { get; } = Behaviors;
}