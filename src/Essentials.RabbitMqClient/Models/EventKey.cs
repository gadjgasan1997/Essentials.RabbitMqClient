using System.Text;
using Essentials.Utils.Extensions;
// ReSharper disable MemberCanBePrivate.Global

namespace Essentials.RabbitMqClient.Models;

/// <summary>
/// Ключ события
/// </summary>
public record EventKey
{
    private EventKey(string? eventTypeName)
    {
        eventTypeName.CheckNotNullOrEmpty(
            $"Название типа события для ключа не может быть пустым: '{eventTypeName}'");
        
        EventTypeName = eventTypeName.FullTrim()!.ToLowerInvariant();
    }
    
    /// <summary>
    /// Название типа события
    /// </summary>
    public string EventTypeName { get; }

    /// <summary>
    /// Создает ключ по названию типа события
    /// </summary>
    /// <param name="eventTypeName">Название типа события</param>
    /// <returns>Ключ</returns>
    internal static EventKey New(string eventTypeName) => new(eventTypeName);

    /// <summary>
    /// Создает ключ по типу события
    /// </summary>
    /// <typeparam name="TEvent">Тип события</typeparam>
    /// <returns>Ключ</returns>
    internal static EventKey New<TEvent>() => new(typeof(TEvent).FullName);
    
    /// <summary>
    /// Возвращает строковое представление ключа
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var builder = new StringBuilder();

        builder
            .Append("{ \"Название типа события\": \"")
            .Append(EventTypeName)
            .Append("\" }");
        
        return builder.ToString();
    }
}