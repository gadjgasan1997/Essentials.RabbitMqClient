using Essentials.Utils.Extensions;
using static Essentials.RabbitMqClient.Dictionaries.KnownExchanges;

namespace Essentials.RabbitMqClient.Configuration.Builders;

/// <summary>
/// Билдер соединения с RabbitMq
/// </summary>
public class ConnectionBuilder : ConfigurationManager
{
    internal ConnectionBuilder(string connectionName)
        : base(connectionName)
    { }
    
    /// <summary>
    /// Объявляет очередь
    /// </summary>
    /// <param name="queueName">Название очереди</param>
    /// <param name="configureAction">Делегат конфигурации очереди</param>
    /// <returns>Билдер соединения с RabbitMq</returns>
    public ConnectionBuilder DeclareQueue(string queueName, Action<QueueBuilder> configureAction)
    {
        configureAction.CheckNotNull($"Делегат конфигурации очереди '{queueName}' не может быть пустым");
        
        var builder = new QueueBuilder(queueName);
        configureAction(builder);
        
        var options = builder.Build();
        ModelOptions.Queues.Add(options);
        
        return this;
    }

    /// <summary>
    /// Объявляет обменник
    /// </summary>
    /// <param name="exchangeName">Название обменника</param>
    /// <param name="exchangeType">Тип обменника</param>
    /// <param name="configureAction">Делегат конфигурации обменника</param>
    /// <returns>Билдер соединения с RabbitMq</returns>
    public ConnectionBuilder DeclareExchange(
        string exchangeName,
        string exchangeType,
        Action<ExchangeBuilder> configureAction)
    {
        configureAction.CheckNotNull($"Делегат конфигурации обменника '{exchangeName}' не может быть пустым");

        var builder = new ExchangeBuilder(exchangeName, exchangeType);
        configureAction(builder);
        
        var options = builder.Build();
        ModelOptions.Exchanges.Add(options);
        
        return this;
    }
    
    /// <summary>
    /// Подписывается на событие
    /// </summary>
    /// <param name="queueName">Название очереди</param>
    /// <param name="routingKey">Ключ маршрутизации</param>
    /// <param name="configureAction">Делегат конфигурации подписки</param>
    /// <typeparam name="TEvent">Тип события</typeparam>
    /// <typeparam name="THandler">Тип обработчика события</typeparam>
    /// <returns>Билдер соединения с RabbitMq</returns>
    public ConnectionBuilder SubscribeToEvent<TEvent, THandler>(
        string queueName,
        string routingKey,
        Action<SubscriptionBuilder<TEvent, THandler>>? configureAction = null)
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>
    {
        var builder = new SubscriptionBuilder<TEvent, THandler>(queueName, routingKey);
        configureAction?.Invoke(builder);
        
        var options = builder.Build();
        ModelOptions.SubscriptionsOptions.Add(options);
        
        return this;
    }
    
    /// <summary>
    /// Настраивает пубилкацию события
    /// </summary>
    /// <param name="routingKey">Ключ маршрутизации</param>
    /// <param name="exchangeName">Название обменника</param>
    /// <param name="configureAction">Делегат конфигурации пубилкации</param>
    /// <typeparam name="TEvent">Тип события</typeparam>
    /// <returns>Билдер соединения с RabbitMq</returns>
    public ConnectionBuilder ConfigurePublishEvent<TEvent>(
        string routingKey,
        string exchangeName = AMQ_DIRECT,
        Action<PublishBuilder<TEvent>>? configureAction = null)
        where TEvent : IEvent
    {
        routingKey.CheckNotNullOrEmpty("Ключ маршрутизации для конфигурации не может быть пустым");

        var builder = new PublishBuilder<TEvent>(exchangeName, routingKey);
        configureAction?.Invoke(builder);
        
        var options = builder.Build();
        ModelOptions.PublishOptions.Add(options);
        
        return this;
    }
    
    /// <summary>
    /// Настраивает Rpc вызовы
    /// </summary>
    /// <param name="configureAction">Делегат конфигурации вызовов</param>
    /// <returns>Билдер соединения с RabbitMq</returns>
    public ConnectionBuilder ConfigureRpc(Action<RpcRequestsBuilder> configureAction)
    {
        configureAction(new RpcRequestsBuilder(this));
        return this;
    }
}