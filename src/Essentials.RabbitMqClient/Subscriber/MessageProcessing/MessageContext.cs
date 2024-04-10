using LanguageExt;
using Essentials.Utils.Extensions;
using RabbitMQ.Client.Events;
using Essentials.RabbitMqClient.Models;
using Essentials.RabbitMqClient.Extensions;
using Essentials.RabbitMqClient.Subscriber.Models;

namespace Essentials.RabbitMqClient.Subscriber.MessageProcessing;
    
/// <summary>
/// Контекст обрабатываемого сообщения
/// </summary>
public static class MessageContext
{
    private static readonly AsyncLocal<Context> _current = new();
    
    /// <summary>
    /// Контекст текущего сообщения
    /// </summary>
    public static Context? Current
    {
        get => _current.Value;
        private set => _current.Value = value.CheckNotNull();
    }

    /// <summary>
    /// Возвращает параметр ReplyTo
    /// </summary>
    /// <returns></returns>
    public static Option<string> GetReplyTo() => Current?.EventArgs.GetReplyTo() ?? Option<string>.None;

    /// <summary>
    /// Возвращает параметр CorrelationId
    /// </summary>
    /// <returns></returns>
    public static Option<string> GetCorrelationId() => Current?.EventArgs.GetCorrelationId() ?? Option<string>.None;

    /// <summary>
    /// Создает контекст сообщения
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <param name="subscriptionKey">Ключ подписки на событие</param>
    /// <param name="eventArgs">Аргументы события</param>
    internal static void CreateContext(
        ConnectionKey connectionKey,
        SubscriptionKey subscriptionKey,
        BasicDeliverEventArgs eventArgs)
    {
        Current ??= new Context(connectionKey, subscriptionKey, eventArgs);
    }
    
    /// <summary>
    /// Контекст сообщения
    /// </summary>
    public class Context
    {
        internal Context(
            ConnectionKey connectionKey,
            SubscriptionKey subscriptionKey,
            BasicDeliverEventArgs eventArgs)
        {
            ConnectionKey = connectionKey.CheckNotNull();
            SubscriptionKey = subscriptionKey.CheckNotNull();
            EventArgs = eventArgs.CheckNotNull();
        }
    
        /// <summary>
        /// Ключ соединения
        /// </summary>
        public ConnectionKey ConnectionKey { get; }
    
        /// <summary>
        /// Ключ подписки на событие
        /// </summary>
        public SubscriptionKey SubscriptionKey { get; }
    
        /// <summary>
        /// Аргументы события
        /// </summary>
        public BasicDeliverEventArgs EventArgs { get; }
    }
}