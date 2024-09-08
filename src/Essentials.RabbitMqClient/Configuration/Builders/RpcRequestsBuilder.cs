using MediatR;
using Essentials.RabbitMqClient.Publisher.MessageProcessing.Behaviors;
using Essentials.RabbitMqClient.Subscriber.MessageProcessing.Behaviors;
using static Essentials.RabbitMqClient.Dictionaries.KnownExchanges;

namespace Essentials.RabbitMqClient.Configuration.Builders;

/// <summary>
/// Билдер Rpc запросов
/// </summary>
public class RpcRequestsBuilder
{
    private readonly ConnectionBuilder _connectionBuilder;

    internal RpcRequestsBuilder(ConnectionBuilder connectionBuilder)
    {
        _connectionBuilder = connectionBuilder;
    }
    
    /// <summary>
    /// Объявляет очередь для получения ответа на Rpc запрос по-умолчанию
    /// </summary>
    /// <param name="queueName">Название очереди</param>
    /// <returns>Билдер соединения с RabbitMq</returns>
    public RpcRequestsBuilder DeclareQueueForRpcResponseDefault(string queueName)
    {
        _connectionBuilder.DeclareQueue(
            queueName,
            queueBuilder =>
                queueBuilder
                    .Exclusive()
                    .AutoDelete()
                    .BindToAmqDirect(queueName));

        return this;
    }

    /// <summary>
    /// Настраивает Rpc запрос
    /// </summary>
    /// <param name="routingKey">Ключ маршрутизации, с которым будет публиковаться сообщение</param>
    /// <param name="replyTo">Ключ маршрутизации, с которым требуется ответить на запрос</param>
    /// <param name="exchangeName">Название обменника</param>
    /// <param name="configureAction">Делегат конфигурации Rpc запроса</param>
    /// <typeparam name="TEvent">Тип события</typeparam>
    /// <returns>Билдер соединения с RabbitMq</returns>
    public RpcRequestsBuilder ConfigureRpcRequest<TEvent>(
        string routingKey,
        string replyTo,
        string exchangeName = AMQ_DIRECT,
        Action<RpcRequestBuilder<TEvent>>? configureAction = null)
        where TEvent : IEvent
    {
        var builder = new RpcRequestBuilder<TEvent>(exchangeName, routingKey, replyTo);
        configureAction?.Invoke(builder);

        var options = builder.Build();
        _connectionBuilder.ModelOptions.RpcRequestsOptions.Add(options);
        
        return this;
    }
    
    /// <summary>
    /// Подписывается на событие ответа на Rpc запрос.
    /// Вызывается со стороны клиента для обработки ответа на Rpc запрос
    /// </summary>
    /// <param name="queueName">Название очереди</param>
    /// <param name="routingKey">Ключ маршрутизации</param>
    /// <param name="configureAction">Делегат конфигурации подписки</param>
    /// <typeparam name="TEvent">Тип события</typeparam>
    /// <returns>Билдер соединения с RabbitMq</returns>
    public RpcRequestsBuilder SubscribeToRpcResponseEvent<TEvent>(
        string queueName,
        string routingKey,
        Action<RpcResponseSubscriptionBuilder<TEvent>>? configureAction = null)
        where TEvent : IEvent
    {
        var builder = new RpcResponseSubscriptionBuilder<TEvent>(queueName, routingKey);
        configureAction?.Invoke(builder);
        
        var options = builder.Build();
        _connectionBuilder.ModelOptions.SubscriptionsOptions.Add(options);
        
        return this;
    }
    
    /// <summary>
    /// Настраивает Rpc запрос по-умолчанию
    /// </summary>
    /// <param name="replyQueueName">Очередь, в которой будет ожидаться ответ</param>
    /// <param name="publishRoutingKey">Ключ маршрутизации, с которым будет публиковаться сообщение</param>
    /// <param name="configurePublishEventAction">Делегат конфигурации публикации сообщения</param>
    /// <param name="configureProcessingResponseAction">Делегат конфигурации обработки ответа</param>
    /// <typeparam name="TEvent">Тип публикуемого события</typeparam>
    /// <typeparam name="TResponse">Тип ожидаемого ответа</typeparam>
    /// <returns>Билдер соединения с RabbitMq</returns>
    public RpcRequestsBuilder ConfigureRpcRequestDefault<TEvent, TResponse>(
        string replyQueueName,
        string publishRoutingKey,
        Action<RpcRequestBuilder<TResponse>>? configurePublishEventAction = null,
        Action<RpcResponseSubscriptionBuilder<TEvent>>? configureProcessingResponseAction = null)
        where TEvent : IEvent
        where TResponse : IEvent
    {
        configurePublishEventAction ??= builder =>
            builder
                .AttachBehavior<LoggingMessagePublisherBehavior>()
                .AttachBehavior<MetricsMessagePublisherBehavior>();
        
        configureProcessingResponseAction ??= builder =>
            builder
                .AttachBehavior<LoggingMessageHandlerBehavior>()
                .AttachBehavior<MetricsMessageHandlerBehavior>();
        
        return DeclareQueueForRpcResponseDefault(replyQueueName)
            .ConfigureRpcRequest(
                routingKey: publishRoutingKey,
                replyTo: replyQueueName,
                configureAction: configurePublishEventAction)
            .SubscribeToRpcResponseEvent(
                replyQueueName,
                routingKey: replyQueueName,
                configureAction: configureProcessingResponseAction);
    }

    /// <summary>
    /// Подписывается на событие Rpc запроса.
    /// Вызывается со стороны сервера для обработки Rpc запроса от клиента
    /// </summary>
    /// <param name="queueName">Название очереди</param>
    /// <param name="routingKey">Ключ маршрутизации</param>
    /// <param name="configureAction">Делегат конфигурации подписки</param>
    /// <typeparam name="TEvent">Тип события</typeparam>
    /// <typeparam name="THandler">Тип обработчика события</typeparam>
    /// <returns>Билдер соединения с RabbitMq</returns>
    public RpcRequestsBuilder SubscribeToRpcRequestEvent<TEvent, THandler>(
        string queueName,
        string routingKey,
        Action<RpcRequestSubscriptionBuilder<TEvent, THandler>>? configureAction = null)
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>
    {
        var builder = new RpcRequestSubscriptionBuilder<TEvent, THandler>(queueName, routingKey);
        configureAction?.Invoke(builder);
        
        var options = builder.Build();
        _connectionBuilder.ModelOptions.SubscriptionsOptions.Add(options);
        
        return this;
    }
    
    /// <summary>
    /// Подписывается на событие Rpc запроса.
    /// Вызывается со стороны сервера для обработки Rpc запроса от клиента
    /// </summary>
    /// <param name="queueName">Название очереди</param>
    /// <param name="routingKey">Ключ маршрутизации</param>
    /// <param name="configureAction">Делегат конфигурации подписки</param>
    /// <typeparam name="TEvent">Тип события</typeparam>
    /// <typeparam name="TResponse">Тип ответа</typeparam>
    /// <returns>Билдер соединения с RabbitMq</returns>
    public RpcRequestsBuilder SubscribeToRpcRequestEvent<TEvent, TResponse>(
        string queueName,
        string routingKey,
        Action<RpcRequestSubscriptionBuilder<TEvent, RabbitMqRpcRequestHandler<TEvent, TResponse>>>? configureAction = null)
        where TEvent : IEvent, IRequest<TResponse>
        where TResponse : IEvent, INotification
    {
        return SubscribeToRpcRequestEvent<TEvent, RabbitMqRpcRequestHandler<TEvent, TResponse>>(
            queueName,
            routingKey,
            configureAction);
    }
    
    /// <summary>
    /// Настраивает пубилкацию события
    /// </summary>
    /// <param name="exchangeName">Название обменника</param>
    /// <param name="configureAction">Делегат конфигурации пубилкации</param>
    /// <typeparam name="TEvent">Тип события</typeparam>
    /// <returns>Билдер соединения с RabbitMq</returns>
    public RpcRequestsBuilder ConfigurePublishRpcResponseEvent<TEvent>(
        string exchangeName = AMQ_DIRECT,
        Action<PublishRpcResponseBuilder<TEvent>>? configureAction = null)
        where TEvent : IEvent
    {
        var builder = new PublishRpcResponseBuilder<TEvent>(exchangeName);
        configureAction?.Invoke(builder);
        
        var options = builder.Build();
        _connectionBuilder.ModelOptions.PublishOptions.Add(options);
        
        return this;
    }
    
    /// <summary>
    /// Настраивает обработку Rpc запроса по-умолчанию
    /// </summary>
    /// <param name="queueName">Название очереди</param>
    /// <param name="configureProcessingEventAction">Делегат конфигурации обработки сообщения</param>
    /// <param name="configurePublishResponseAction">Делегат конфигурации публикации ответа</param>
    /// <typeparam name="TEvent">Тип публикуемого события</typeparam>
    /// <typeparam name="TResponse">Тип ожидаемого ответа</typeparam>
    /// <returns>Билдер соединения с RabbitMq</returns>
    public RpcRequestsBuilder ConfigureRpcRequestHandlingDefault<TEvent, TResponse>(
        string queueName,
        Action<RpcRequestSubscriptionBuilder<TEvent, RabbitMqRpcRequestHandler<TEvent, TResponse>>>? configureProcessingEventAction = null,
        Action<PublishRpcResponseBuilder<TResponse>>? configurePublishResponseAction = null)
        where TEvent : IEvent, IRequest<TResponse>
        where TResponse : IEvent, INotification
    {
        _connectionBuilder.DeclareQueue(queueName, queueBuilder => queueBuilder.Durable().BindToAmqDirect(queueName));
        
        configureProcessingEventAction ??= builder =>
            builder
                .AttachBehavior<LoggingMessageHandlerBehavior>()
                .AttachBehavior<MetricsMessageHandlerBehavior>();
        
        configurePublishResponseAction ??= builder =>
            builder
                .AttachBehavior<LoggingMessagePublisherBehavior>()
                .AttachBehavior<MetricsMessagePublisherBehavior>();
        
        return SubscribeToRpcRequestEvent(
                queueName,
                routingKey: queueName,
                configureAction: configureProcessingEventAction)
            .ConfigurePublishRpcResponseEvent(
                configureAction: configurePublishResponseAction);
    }
}