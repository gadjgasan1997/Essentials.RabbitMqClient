using Polly;
using Essentials.Utils.Extensions;
using Essentials.Serialization;
using System.Net.Sockets;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using Essentials.RabbitMqClient.Extensions;
using Essentials.RabbitMqClient.Dictionaries;
using Essentials.RabbitMqClient.Exceptions;
using Essentials.RabbitMqClient.Models;
using Essentials.RabbitMqClient.Context;
using Essentials.RabbitMqClient.Publisher.Models;
using Essentials.RabbitMqClient.RabbitMqConnections;
using Essentials.RabbitMqClient.Publisher.MessageProcessing;
using SubscribeMessageContext = Essentials.RabbitMqClient.Subscriber.MessageProcessing.MessageContext;
using PublishMessageContext = Essentials.RabbitMqClient.Publisher.MessageProcessing.MessageContext;
using static System.Environment;
using static LanguageExt.Prelude;
using static Essentials.Serialization.Helpers.JsonHelpers;
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace Essentials.RabbitMqClient.Publisher.Implementations;

/// <inheritdoc cref="IEventsPublisher" />
[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
internal class EventsPublisher : IEventsPublisher
{
    private readonly IChannelFactory _channelFactory;
    private readonly IOptionsProvider _provider;
    private readonly IAskManager _askManager;
    private readonly IContextService _contextService;
    private readonly IEnumerable<IMessagePublishBehavior> _behaviors;
    private readonly ILogger _logger;

    public EventsPublisher(
        IChannelFactory channelFactory,
        IOptionsProvider optionsProvider,
        IAskManager askManager,
        IContextService contextService,
        IEnumerable<IMessagePublishBehavior> behaviors,
        ILoggerFactory loggerFactory)
    {
        _channelFactory = channelFactory.CheckNotNull();
        _provider = optionsProvider.CheckNotNull();
        _askManager = askManager.CheckNotNull();
        _contextService = contextService.CheckNotNull();
        _behaviors = behaviors;
        _logger = loggerFactory.CreateLogger("Essentials.RabbtMqClient.EventsPublisher");
    }

    /// <inheritdoc cref="IEventsPublisher.Publish{TEvent}(TEvent)" />
    public void Publish<TEvent>(TEvent @event)
        where TEvent : IEvent
    {
        var eventKey = EventKey.New<TEvent>();
        
        var connectionKey = _provider.GetConnectionKeyForPublishEvent(eventKey);
        var exchange = _provider.GetExchangeForPublishEvent(connectionKey, eventKey);

        var key = _provider.GetRoutingKeyForPublishEvent(connectionKey, eventKey);

        var routingKey = RoutingKey.New(key);
        var publishKey = PublishKey.New(exchange, eventKey, routingKey);
        var publishOptions = _provider.GetPublishOptions(connectionKey, publishKey);
        
        PublishPrivate(
            @event,
            connectionKey,
            publishKey,
            publishOptions.Behaviors,
            publishOptions.ContentType,
            publishOptions.RetryCount,
            propertiesFunc: channel =>
            {
                var properties = channel.CreateBasicProperties();
                properties.DeliveryMode = publishOptions.DeliveryMode;
                return properties;
            });
    }

    /// <inheritdoc cref="IEventsPublisher.Publish{TEvent}(TEvent, PublishParams)" />
    public void Publish<TEvent>(TEvent @event, PublishParams publishParams)
        where TEvent : IEvent
    {
        var eventKey = EventKey.New<TEvent>();
        
        var connectionKey = _provider.GetConnectionKeyForPublishEvent(eventKey, publishParams);
        var exchange = publishParams.Exchange ?? _provider.GetExchangeForPublishEvent(connectionKey, eventKey);
        var key = publishParams.RoutingKey ?? _provider.GetRoutingKeyForPublishEvent(connectionKey, eventKey);

        var routingKey = RoutingKey.New(key);
        var publishKey = PublishKey.New(exchange, eventKey, routingKey);
        var publishOptions = _provider.GetPublishOptions(connectionKey, publishKey);

        PublishPrivate(
            @event,
            connectionKey,
            publishKey,
            publishOptions.Behaviors,
            publishOptions.ContentType,
            publishOptions.RetryCount,
            propertiesFunc: channel =>
            {
                var properties = channel.CreateBasicProperties();
                properties.DeliveryMode = publishOptions.DeliveryMode;
                return properties;
            });
    }

    /// <inheritdoc cref="IEventsPublisher.PublishRpcResponse{TEvent}(TEvent)" />
    public void PublishRpcResponse<TEvent>(TEvent @event)
        where TEvent : IEvent
    {
        var context = SubscribeMessageContext.Current
            .CheckNotNull(
                "Не инициализирован контекст для отдачи ответа",
                "MessageContext.Current");
        
        var eventKey = EventKey.New<TEvent>();
        
        var connectionKey = context.ConnectionKey;
        var exchange = _provider.GetExchangeForPublishEvent(connectionKey, eventKey);

        var replyTo = context.EventArgs
            .GetReplyTo()
            .IfNone(() => throw new InvalidOperationException(
                "Не указан параметр ReplyTo в аргументах события для публикации ответа. " +
                $"Параметры события: '{Serialize(context.EventArgs)}'"));
        
        var correlationId = context.EventArgs
            .GetCorrelationId()
            .IfNone(() => throw new InvalidOperationException(
                "Не указан параметр CorrelationId в аргументах события для публикации ответа. " +
                $"Параметры события: '{Serialize(context.EventArgs)}'"));

        var routingKey = RoutingKey.New(replyTo);
        var publishKey = PublishKey.New(exchange, eventKey, routingKey);
        
        // Поиск опций публикации осуществляется без replyTo, так как данный параметр неизвестен
        // на этапе запуска сервиса, а приходит от сторонней системы.
        // Соответственно, опций публикации для такого ключа маршрутизации не будет в конфигурации сервиса
        var publishOptions = _provider.GetPublishOptions(connectionKey, PublishKey.New(exchange, eventKey));

        PublishPrivate(
            @event,
            connectionKey,
            publishKey,
            publishOptions.Behaviors,
            publishOptions.ContentType,
            publishOptions.RetryCount,
            propertiesFunc: channel =>
            {
                var properties = channel.CreateBasicProperties();
                properties.DeliveryMode = publishOptions.DeliveryMode;
                properties.CorrelationId = correlationId;
                return properties;
            });
    }

    /// <inheritdoc cref="IEventsPublisher.AskAsync{TEvent, TAnswer}(TEvent, CancellationToken)" />
    public async Task<Result<TAnswer>> AskAsync<TEvent, TAnswer>(TEvent @event, CancellationToken token)
        where TEvent : IEvent
        where TAnswer : IEvent
    {
        return await TryAsync(async () =>
            {
                var eventKey = EventKey.New<TEvent>();
        
                var connectionKey = _provider.GetConnectionKeyForPublishEvent(eventKey);
                var exchange = _provider.GetExchangeForPublishEvent(connectionKey, eventKey);
                var key = _provider.GetRoutingKeyForPublishEvent(connectionKey, eventKey);
        
                var routingKey = RoutingKey.New(key);
                var publishKey = PublishKey.New(exchange, eventKey, routingKey);
                var rpcRequestOptions = _provider.GetRpcRequestOptions(connectionKey, publishKey);
            
                if (token is { CanBeCanceled: true, IsCancellationRequested: true })
                    throw new InvalidAskAttemptException("Публикация сообщения была отменена");

                var result = await AskAsyncPrivate(
                    @event,
                    connectionKey,
                    publishKey,
                    rpcRequestOptions);
                
                return (TAnswer) result;
            })
            .Map(result => new Result<TAnswer>(result))
            .IfFail(exception =>
            {
                _logger.LogWarning(exception, "Произошла ошибка при попытке получить ответ из очереди");
                return new Result<TAnswer>(exception);
            });
    }

    /// <inheritdoc cref="IEventsPublisher.AskAsync{TEvent, TAnswer}(TEvent, PublishParams, CancellationToken)" />
    public async Task<Result<TAnswer>> AskAsync<TEvent, TAnswer>(
        TEvent @event,
        PublishParams publishParams,
        CancellationToken token)
        where TEvent : IEvent
        where TAnswer : IEvent
    {
        return await TryAsync(async () =>
            {
                var eventKey = EventKey.New<TEvent>();

                var connectionKey = _provider.GetConnectionKeyForPublishEvent(eventKey, publishParams);
                var exchange = publishParams.Exchange ?? _provider.GetExchangeForPublishEvent(connectionKey, eventKey);
                var key = publishParams.RoutingKey ?? _provider.GetRoutingKeyForPublishEvent(connectionKey, eventKey);

                var routingKey = RoutingKey.New(key);
                var publishKey = PublishKey.New(exchange, eventKey, routingKey);
                var rpcRequestOptions = _provider.GetRpcRequestOptions(connectionKey, publishKey);

                if (token is { CanBeCanceled: true, IsCancellationRequested: true })
                    throw new InvalidAskAttemptException("Публикация сообщения была отменена");

                var result = await AskAsyncPrivate(
                    @event,
                    connectionKey,
                    publishKey,
                    rpcRequestOptions);
                
                return (TAnswer) result;
            })
            .Map(result => new Result<TAnswer>(result))
            .IfFail(exception =>
            {
                _logger.LogWarning(
                    exception,
                    $"Произошла ошибка при попытке получить ответ из очереди с параметрами: {Serialize(publishParams)}");

                return new Result<TAnswer>(exception);
            });
    }

    private async Task<IEvent> AskAsyncPrivate<TEvent>(
        TEvent @event,
        ConnectionKey connectionKey,
        PublishKey publishKey,
        RpcRequestOptions rpcRequestOptions)
        where TEvent : IEvent
    {
        var correlationId = Guid.NewGuid().ToString("N");
            
        _contextService.SaveScopeProperties(correlationId);
        
        return await _askManager
            .GetCreateAsk(correlationId)
            .MapAsync(async tcs =>
            {
                new CancellationTokenSource(rpcRequestOptions.Timeout)
                    .Token
                    .Register(() =>
                    {
                        if (tcs.Task.IsCompleted) return;
                        _logger.LogInformation($"Получен токен отмены: сообщение '{correlationId}' будет удалено.");

                        _ = _askManager
                            .Cancel(
                                correlationId,
                                ex: new TimeoutException(
                                    $"Не удалось получить ответ за {rpcRequestOptions.Timeout}"))
                            .IfFailThrow();
                    });
                
                PublishPrivate(
                    @event,
                    connectionKey,
                    publishKey,
                    rpcRequestOptions.Behaviors,
                    rpcRequestOptions.ContentType,
                    rpcRequestOptions.RetryCount,
                    propertiesFunc: channel =>
                    {
                        var properties = channel.CreateBasicProperties();
                        
                        properties.DeliveryMode = rpcRequestOptions.DeliveryMode;
                        properties.CorrelationId = correlationId;
                        properties.ReplyTo = rpcRequestOptions.ReplyTo.Key;
                        
                        return properties;
                    });
                    
                return await tcs.Task;
            })
            .IfFailThrow();
    }

    private void PublishPrivate<TEvent>(
        TEvent @event,
        ConnectionKey connectionKey,
        PublishKey publishKey,
        IEnumerable<Type> behaviors,
        string contentType,
        int retryCount,
        Func<IModel, IBasicProperties> propertiesFunc)
        where TEvent : IEvent
    {
        PublishMessageContext.CreateContext(connectionKey, publishKey, @event);
        
        behaviors
            .Select(type => _behaviors.FirstOrDefault(behavior => behavior.GetType() == type))
            .OfType<IMessagePublishBehavior>()
            .Aggregate(
                (MessagePublishDelegate) SeedPublisher,
                (next, behavior) => async () => await behavior.Handle(next))
            .Invoke();
        
        return;

        Task SeedPublisher()
        {
            var message = GetMessage(@event, contentType);
            var channel = _channelFactory.GetOrCreateChannelForPublish(connectionKey);
            var properties = propertiesFunc(channel);
            
            PublishWithPolicy(
                channel,
                message,
                publishKey,
                retryCount,
                properties);

            return Task.CompletedTask;
        }
    }

    private static byte[] GetMessage<TEvent>(TEvent @event, string contentType)
    {
        var serializer = contentType switch
        {
            MessageContentType.JSON => EssentialsSerializersFactory.TryGet(KnownRabbitMqSerializers.JSON),
            MessageContentType.XML => EssentialsSerializersFactory.TryGet(KnownRabbitMqSerializers.XML),
            _ => throw new KeyNotFoundException(
                $"Не найден сериалайзер для типа содержимого '{contentType}'")
        };
        
        return serializer.Serialize(@event);
    }

    private void PublishWithPolicy(
        IModel channel,
        byte[] message,
        PublishKey publishKey,
        int retryCount,
        IBasicProperties properties)
    {
        Policy
            .Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .Or<Exception>()
            .WaitAndRetry(
                retryCount: retryCount,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, time) =>
                {
                    _logger.LogError(
                        exception,
                        $"Не удалось опубликовать сообщение по происшествии {time.TotalSeconds:n1} секунд." +
                        $"{NewLine}Exhange: '{publishKey.Exchange}'." +
                        $"{NewLine}Routing key: '{publishKey.RoutingKey?.Key}'.");
                })
            .Execute(() =>
            {
                channel.BasicPublish(
                    exchange: publishKey.Exchange,
                    routingKey: publishKey.RoutingKey?.Key,
                    mandatory: true,
                    basicProperties: properties,
                    body: message);
            });
    }
}