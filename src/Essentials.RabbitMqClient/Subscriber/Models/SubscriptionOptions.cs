using Essentials.Utils.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace Essentials.RabbitMqClient.Subscriber.Models;

/// <summary>
/// Опции подписки на очередь
/// </summary>
internal record SubscriptionOptions
{
    public SubscriptionOptions(
        string eventTypeName,
        string contentType,
        ushort prefetchCount,
        List<Type> behaviors)
    {
        EventTypeName = eventTypeName.CheckNotNullOrEmpty();
        ContentType = contentType.CheckNotNullOrEmpty();
        PrefetchCount = prefetchCount;
        NeedCorrelation = true;
        Behaviors = behaviors;
    }
    
    public SubscriptionOptions(
        string eventTypeName,
        Type handlerType,
        string contentType,
        ushort prefetchCount,
        List<Type> behaviors)
    {
        EventTypeName = eventTypeName.CheckNotNullOrEmpty();
        HandlerType = handlerType.CheckNotNull();
        ContentType = contentType.CheckNotNullOrEmpty();
        PrefetchCount = prefetchCount;
        NeedCorrelation = false;
        Behaviors = behaviors;
    }
    
    /// <summary>
    /// Название типа события
    /// </summary>
    public string EventTypeName { get; }
    
    /// <summary>
    /// Тип обработчика события
    /// </summary>
    public Type? HandlerType { get; }
    
    /// <summary>
    /// Тип содержимого сообщения
    /// </summary>
    public string ContentType { get; }

    /// <summary>
    /// Количество сообщений, которое может забрать подписчик
    /// </summary>
    public ushort PrefetchCount { get; }

    /// <summary>
    /// Признак необходимости установить событие в задаче (RPC)
    /// </summary>
    [MemberNotNullWhen(false, nameof(HandlerType))]
    public bool NeedCorrelation { get; }
    
    /// <summary>
    /// Список перехватчиков сообщения
    /// </summary>
    public List<Type> Behaviors { get; }
}