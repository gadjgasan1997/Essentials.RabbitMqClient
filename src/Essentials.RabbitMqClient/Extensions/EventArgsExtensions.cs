using System.Text;
using LanguageExt;
using RabbitMQ.Client.Events;
// ReSharper disable ConvertIfStatementToReturnStatement

namespace Essentials.RabbitMqClient.Extensions;

/// <summary>
/// Методы расширения для <see cref="BasicDeliverEventArgs" />
/// </summary>
internal static class EventArgsExtensions
{
    /// <summary>
    /// Пытается вернуть параметр CorrelationId
    /// </summary>
    /// <param name="eventArgs">Аргументы события</param>
    /// <returns></returns>
    public static Option<string> GetCorrelationId(
        this BasicDeliverEventArgs? eventArgs)
    {
        if (eventArgs?.BasicProperties is null)
            return Option<string>.None;
        
        if (eventArgs.BasicProperties.Headers is not null)
        {
            if (eventArgs.BasicProperties.Headers.TryGetValue("rabbitmq.message.correlation.id", out var value))
                return Encoding.UTF8.GetString((byte[]) value);
        }
        
        if (!string.IsNullOrWhiteSpace(eventArgs.BasicProperties.CorrelationId))
            return eventArgs.BasicProperties.CorrelationId;

        return Option<string>.None;
    }
    
    /// <summary>
    /// Пытается вернуть параметр ReplyTo
    /// </summary>
    /// <param name="eventArgs">Аргументы события</param>
    /// <returns></returns>
    public static Option<string> GetReplyTo(
        this BasicDeliverEventArgs? eventArgs)
    {
        if (eventArgs?.BasicProperties is null)
            return Option<string>.None;
        
        if (!string.IsNullOrWhiteSpace(eventArgs.BasicProperties.ReplyTo))
            return eventArgs.BasicProperties.ReplyTo;

        return Option<string>.None;
    }
}