using Essentials.Utils.Extensions;

namespace Essentials.RabbitMqClient.Subscriber.Models;

/// <summary>
/// Опции регистрации обработчиков
/// </summary>
internal record RegisterEventsHandlersOptions
{
    public RegisterEventsHandlersOptions(string eventTypeName, Type handlerType)
    {
        EventTypeName = eventTypeName.CheckNotNullOrEmpty();
        HandlerType = handlerType;
    }
    
    /// <summary>
    /// Название типа события
    /// </summary>
    public string EventTypeName { get; }
    
    /// <summary>
    /// Тип обработчика события
    /// </summary>
    public Type HandlerType { get; }
}