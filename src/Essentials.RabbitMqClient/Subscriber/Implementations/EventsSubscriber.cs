using Essentials.Utils.Extensions;
using Essentials.RabbitMqClient.Context;
using Essentials.RabbitMqClient.Exceptions;
using Essentials.RabbitMqClient.Extensions;
using RabbitMQ.Client.Events;
using Essentials.RabbitMqClient.OptionsProvider;
using Essentials.RabbitMqClient.RabbitMqConnections;
using Essentials.RabbitMqClient.Subscriber.MessageProcessing;
using Essentials.RabbitMqClient.Subscriber.Models;
using static System.Environment;
using static Essentials.Serialization.Helpers.JsonHelpers;
using static Essentials.RabbitMqClient.Dictionaries.QueueLoggers;

namespace Essentials.RabbitMqClient.Subscriber.Implementations;

/// <inheritdoc cref="IEventsSubscriber" />
internal class EventsSubscriber : IEventsSubscriber
{
    private readonly IChannelFactory _channelFactory;
    private readonly IOptionsProvider _optionsProvider;
    private readonly IContextService _contextService;
    private readonly IEventsHandlerService _eventsHandlerService;
    
    public EventsSubscriber(
        IChannelFactory channelFactory,
        IOptionsProvider optionsProvider,
        IContextService contextService,
        IEventsHandlerService eventsHandlerService)
    {
        _channelFactory = channelFactory.CheckNotNull();
        _optionsProvider = optionsProvider.CheckNotNull();
        _contextService = contextService.CheckNotNull();
        _eventsHandlerService = eventsHandlerService.CheckNotNull();
    }
    
    /// <inheritdoc cref="IEventsSubscriber.Subscribe{TEvent}" />
    public void Subscribe<TEvent>(SubscriptionParams subscriptionParams)
        where TEvent : IEvent
    {
        // Получение опций подписки на событие
        var queueKey = subscriptionParams.QueueKey;
        var routingKey = subscriptionParams.RoutingKey;
        var subscriptionKey = SubscriptionKey.New(queueKey, routingKey);
        
        var options = _optionsProvider.GetSubscriptionOptions(subscriptionParams.ConnectionKey, subscriptionKey);

        // Проверка на дублирование обработчика
        var connectionKey = subscriptionParams.ConnectionKey;
        if (_eventsHandlerService.TryGetHandler(connectionKey, subscriptionKey, out _))
        {
            EventsSubscriberLogger.Warn(
                $"Для очереди '{queueKey}' уже имеется подписка на событие с ключом '{subscriptionKey}'." +
                $"{NewLine}Опции подписки: '{Serialize(options)}'.");
            
            return;
        }

        _channelFactory.GetOrCreateChannelForSubscribe(connectionKey, subscriptionKey, options);
        
        var consumer = _channelFactory.CreateConsumer(connectionKey, subscriptionKey);
        consumer.Received += ConsumerReceived;
        
        // Добавление обработчика
        _eventsHandlerService.RegisterEventHandler<TEvent>(connectionKey, subscriptionKey, options);

        EventsSubscriberLogger.Info(
            $"Произошла подписка на событие с ключом '{subscriptionKey}'." +
            $"{NewLine}Опции подписки: '{Serialize(options)}'.");
    }

    /// <summary>
    /// Метод-обработчик события получения сообщения от очереди
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    private async Task ConsumerReceived(object sender, BasicDeliverEventArgs eventArgs)
    {
        if (sender is not AsyncEventingBasicConsumer consumer)
        {
            EventsSubscriberLogger.Error(
                $"Слушатель не является типом '{typeof(AsyncEventingBasicConsumer).FullName}'" +
                $"{NewLine}Аргументы: '{Serialize(eventArgs)}'");

            return;
        }

        if (!_channelFactory.TryGetSubscriptionKeys(
            consumer.ConsumerTags,
            eventArgs.RoutingKey,
            out var connectionKey,
            out var subscriptionKey))
        {
            EventsSubscriberLogger.Error(
                $"Не удалось определить ключ подписки для слушателя: '{Serialize(consumer)}'" +
                $"{NewLine}Аргументы: '{Serialize(eventArgs)}'");

            return;
        }

        MessageContext.CreateContext(connectionKey, subscriptionKey.Value, eventArgs);

        var correlationId = eventArgs.GetCorrelationId().IfNone(string.Empty);
        using var _ = _contextService.RestoreScopeProperties(correlationId);
        
        try
        {
            var options = _optionsProvider.GetSubscriptionOptions(connectionKey, subscriptionKey.Value);
            await _eventsHandlerService.HandleEvent(options);
            
            consumer.Model.BasicAck(eventArgs.DeliveryTag, false);
        }
        catch (HandlerNotFoundException exception)
        {
            EventsSubscriberLogger.Warn(
                exception,
                $"Для события с ключом '{subscriptionKey}' не найден обработчик.");
        }
        catch (Exception exception)
        {
            EventsSubscriberLogger.Error(
                exception,
                $"Во время обработки сообщения из очереди '{eventArgs.ConsumerTag}' " +
                $"с ключом маршрутизации '{eventArgs.RoutingKey}' произошло исключение." +
                $"{NewLine}Параметры сообщения: {Serialize(eventArgs.BasicProperties)}");
            
            consumer.Model.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
        }
    }
}