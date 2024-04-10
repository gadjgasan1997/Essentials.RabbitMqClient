using Essentials.Utils.Extensions;
using Essentials.Serialization;
using Essentials.Serialization.Helpers;
using System.Diagnostics.CodeAnalysis;
using Essentials.RabbitMqClient.Extensions;
using Essentials.RabbitMqClient.Dictionaries;
using Essentials.RabbitMqClient.Exceptions;
using Essentials.RabbitMqClient.Models;
using Essentials.RabbitMqClient.Publisher;
using Essentials.RabbitMqClient.Subscriber.MessageProcessing;
using Essentials.RabbitMqClient.Subscriber.Models;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client.Events;

namespace Essentials.RabbitMqClient.Subscriber.Implementations;

/// <inheritdoc cref="IEventsHandlerService" />
internal class EventsHandlerService : IEventsHandlerService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IAskManager _askManager;
    
    /// <summary>
    /// Мапа ключей маршрутизации на обработчики событий
    /// </summary>
    private readonly Dictionary<(ConnectionKey, SubscriptionKey), HandlerInfo> _handlers = new();

    /// <summary>
    /// Список перехватчиков запроса
    /// </summary>
    private readonly IEnumerable<IMessageHandlerBehavior> _behaviors;

    public EventsHandlerService(
        IServiceScopeFactory scopeFactory,
        IAskManager askManager,
        IEnumerable<IMessageHandlerBehavior> behaviors)
    {
        _scopeFactory = scopeFactory.CheckNotNull();
        _askManager = askManager.CheckNotNull();
        _behaviors = behaviors;
    }

    /// <inheritdoc cref="IEventsHandlerService.TryGetHandler" />
    public bool TryGetHandler(
        ConnectionKey connectionKey,
        SubscriptionKey subscriptionKey,
        [NotNullWhen(true)] out HandlerInfo? handlerInfo)
    {
        return _handlers.TryGetValue((connectionKey, subscriptionKey), out handlerInfo);
    }

    /// <inheritdoc cref="IEventsHandlerService.RegisterEventHandler{TEvent}" />
    public void RegisterEventHandler<TEvent>(
        ConnectionKey connectionKey,
        SubscriptionKey subscriptionKey,
        SubscriptionOptions options)
        where TEvent : IEvent
    {
        var func = options.Correlation
            ? GetCorrelationEnabledHandler<TEvent>(options)
            : GetCorrelationDisabledHandler<TEvent>(options);
        
        _handlers.Add((connectionKey, subscriptionKey), new HandlerInfo(func));
    }
    
    /// <inheritdoc cref="IEventsHandlerService.HandleEvent" />
    public async Task HandleEvent(SubscriptionOptions options)
    {
        var context = MessageContext.Current;
        context.CheckNotNull("Context must not be null here!");
        
        var connectionKey = context.ConnectionKey;
        var subscriptionKey = context.SubscriptionKey;
        var eventArgs = context.EventArgs;
        
        if (!TryGetHandler(connectionKey, subscriptionKey, out var handlerInfo))
            throw new HandlerNotFoundException(subscriptionKey);
        
        await options.Behaviors
            .Select(type => _behaviors.FirstOrDefault(behavior => behavior.GetType() == type))
            .OfType<IMessageHandlerBehavior>()
            .Aggregate(
                (MessageHandlerDelegate) SeedHandler,
                (next, behavior) => async () => await NextHandler(next, behavior))
            .Invoke();

        return;

        async Task SeedHandler() => await handlerInfo.Handler(eventArgs);

        async Task NextHandler(MessageHandlerDelegate next, IMessageHandlerBehavior behavior) =>
            await behavior.Handle(next);
    }

    #region Events Handlers
    
    /// <summary>
    /// Возвращает обработчик события с включенной кореляцией сообщений
    /// </summary>
    /// <param name="options">Опции подписки</param>
    /// <typeparam name="TEvent">Тип события</typeparam>
    /// <returns>Обработчик</returns>
    private Func<BasicDeliverEventArgs, Task> GetCorrelationEnabledHandler<TEvent>(SubscriptionOptions options)
        where TEvent : IEvent
    {
        return eventArgs =>
        {
            var @event = GetEvent<TEvent>(eventArgs.Body, options.ContentType);
            
            var correlationId = eventArgs
                .GetCorrelationId()
                .IfNone(() =>
                    throw new InvalidOperationException(
                        "Не указан параметр CorrelationId в аргументах события для обработки сообщения. " +
                        $"Параметры события: '{JsonHelpers.Serialize(eventArgs)}'"));

            _ = _askManager
                .SetAnswer(correlationId, @event)
                .IfFailThrow();

            return !string.IsNullOrWhiteSpace(options.HandlerTypeName)
                ? BasicHandler(@event)
                : Task.CompletedTask;
        };
    }

    /// <summary>
    /// Возвращает обработчик события с отключенной кореляцией сообщений
    /// </summary>
    /// <param name="options">Опции подписки</param>
    /// <typeparam name="TEvent">Тип события</typeparam>
    /// <returns>Обработчик</returns>
    private Func<BasicDeliverEventArgs, Task> GetCorrelationDisabledHandler<TEvent>(SubscriptionOptions options)
        where TEvent : IEvent
    {
        return eventArgs =>
        {
            var @event = GetEvent<TEvent>(eventArgs.Body, options.ContentType);
            
            return !string.IsNullOrWhiteSpace(options.HandlerTypeName)
                ? BasicHandler(@event)
                : Task.CompletedTask;
        };
    }
    
    /// <summary>
    /// Десерилизует полученное сообщение в объект и делегирует его обработку конкретному хендлеру
    /// </summary>
    /// <param name="event">Тип содержимого сообщения</param>
    /// <typeparam name="TEvent">Тип события</typeparam>
    private async Task BasicHandler<TEvent>(TEvent @event)
        where TEvent : IEvent
    {
        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<TEvent>>();
        await handler.HandleAsync(@event);
    }
    
    /// <summary>
    /// Получает десериализованное сообщение из события
    /// </summary>
    /// <param name="content">Тело сообщения</param>
    /// <param name="contentType">Формат</param>
    /// <typeparam name="TEvent">Тип события</typeparam>
    /// <exception cref="KeyNotFoundException"></exception>
    private static TEvent GetEvent<TEvent>(ReadOnlyMemory<byte> content, string contentType)
        where TEvent : IEvent
    {
        try
        {
            var deserializer = contentType switch
            {
                MessageContentType.JSON => EssentialsDeserializersFactory.TryGet(KnownRabbitMqDeserializers.JSON),
                MessageContentType.XML => EssentialsDeserializersFactory.TryGet(KnownRabbitMqDeserializers.XML),
                _ => throw new KeyNotFoundException($"Не найден десериалайзер для типа содержимого '{contentType}'")
            };

            return deserializer
                .Deserialize<TEvent>(content)
                .CheckNotNull("Сообщение после десериализации равно Null");
        }
        catch (Exception innerException)
        {
            throw new InvalidOperationException(
                "Во время десерилизации сообщения произошло исключение.",
                innerException);
        }
    }
    
    #endregion
}