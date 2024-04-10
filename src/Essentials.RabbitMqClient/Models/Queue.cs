using Essentials.Utils.Extensions;

namespace Essentials.RabbitMqClient.Models;

/// <summary>
/// Очередь
/// </summary>
internal record Queue
{
    public Queue(
        QueueKey queueKey,
        bool durable,
        bool exclusive,
        bool autoDelete,
        List<Binding> bindings)
    {
        QueueKey = queueKey.CheckNotNull();
        Durable = durable;
        Exclusive = exclusive;
        AutoDelete = autoDelete;
        Bindings = bindings;
    }
    
    /// <summary>
    /// Название
    /// </summary>
    public QueueKey QueueKey { get; }

    /// <summary>
    /// Признак, что очередь сохраняет свое состояние
    /// </summary>
    public bool Durable { get; }
    
    /// <summary>
    /// Признак, что очередь разрешает подключаться только одному потребителю
    /// </summary>
    public bool Exclusive { get; }
    
    /// <summary>
    /// Признак, что очередь будет удалена автоматически
    /// </summary>
    public bool AutoDelete { get; }

    /// <summary>
    /// Привязки
    /// </summary>
    public List<Binding> Bindings { get; }
}