using Essentials.Utils.Extensions;
using Essentials.RabbitMqClient.Options;
using static Essentials.RabbitMqClient.Dictionaries.KnownExchanges;

namespace Essentials.RabbitMqClient.Configuration.Builders;

/// <summary>
/// Билдер очереди
/// </summary>
public class QueueBuilder
{
    private readonly string _queueName;
    private bool _isDurable;
    private bool _isExclusive;
    private bool _isAutoDelete;
    private readonly List<BindingOptions> _bindingOptions = [];

    internal QueueBuilder(string queueName)
    {
        _queueName = queueName.CheckNotNullOrEmpty("Название очереди для конфигурации не может быть пустым");
    }
    
    /// <summary>
    /// Указывает, что очередь должна сохранять свое состояние
    /// </summary>
    /// <returns>Билдер очереди</returns>
    public QueueBuilder Durable()
    {
        _isDurable = true;
        return this;
    }
    
    /// <summary>
    /// Указывает, что очередь является эксклюзивной (разрешает подключаться только одному потребителю)
    /// </summary>
    /// <returns>Билдер очереди</returns>
    public QueueBuilder Exclusive()
    {
        _isExclusive = true;
        return this;
    }
    
    /// <summary>
    /// Указывает, что очередь будет удалена автоматически после остановки клиента
    /// </summary>
    /// <returns>Билдер очереди</returns>
    public QueueBuilder AutoDelete()
    {
        _isAutoDelete = true;
        return this;
    }
    
    /// <summary>
    /// Привязывает очередь к обменнику
    /// </summary>
    /// <param name="exchange">Обменник</param>
    /// <param name="routingKey">Ключ маршрутизации</param>
    /// <returns>Билдер очереди</returns>
    public QueueBuilder Bind(string exchange, string routingKey)
    {
        exchange.CheckNotNullOrEmpty("Название обменника для привязки к очереди не может быть пустым");
        routingKey.CheckNotNullOrEmpty(
            $"Ключ маршрутизации для привязки обменника '{exchange}' к очереди не может быть пустым");
        
        _bindingOptions.Add(new BindingOptions
        {
            Exchange = exchange,
            RoutingKey = routingKey
        });
        
        return this;
    }
    
    /// <summary>
    /// Привязывает очередь к обменнику с названием 'amq.direct'
    /// </summary>
    /// <param name="routingKey">Ключ маршрутизации</param>
    /// <returns>Билдер очереди</returns>
    public QueueBuilder BindToAmqDirect(string routingKey) => Bind(AMQ_DIRECT, routingKey);
    
    /// <summary>
    /// Билдит опции очереди
    /// </summary>
    /// <returns>Опции очереди</returns>
    internal QueueOptions Build()
    {
        return new QueueOptions
        {
            Name = _queueName,
            Durable = _isDurable,
            Exclusive = _isExclusive,
            AutoDelete = _isAutoDelete,
            Bindings = _bindingOptions
        };
    }
}