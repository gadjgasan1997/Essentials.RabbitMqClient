using Essentials.Utils.Extensions;
using Essentials.RabbitMqClient.Models;
using Essentials.RabbitMqClient.Publisher.Models;

namespace Essentials.RabbitMqClient.Publisher.MessageProcessing;

/// <summary>
/// Контекст публикуемого сообщения
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
    /// Создает контекст сообщения
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <param name="publishKey">Ключ публикации события</param>
    /// <param name="content">Содержимое</param>
    internal static void CreateContext(
        ConnectionKey connectionKey,
        PublishKey publishKey,
        object content)
    {
        Current ??= new Context(connectionKey, publishKey, content);
    }
    
    /// <summary>
    /// Контекст сообщения
    /// </summary>
    public class Context
    {
        internal Context(
            ConnectionKey connectionKey,
            PublishKey publishKey,
            object content)
        {
            ConnectionKey = connectionKey.CheckNotNull();
            PublishKey = publishKey.CheckNotNull();
            Content = content.CheckNotNull();
        }
    
        /// <summary>
        /// Ключ соединения
        /// </summary>
        public ConnectionKey ConnectionKey { get; }

        /// <summary>
        /// Ключ публикации события
        /// </summary>
        public PublishKey PublishKey { get; }
    
        /// <summary>
        /// Содержимое
        /// </summary>
        public object Content { get; }
    }
}