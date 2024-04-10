using App.Metrics;
using App.Metrics.Counter;
using Essentials.Utils.Extensions;
using System.Collections.Concurrent;
using Essentials.RabbitMqClient.Publisher.Models;
using Essentials.RabbitMqClient.Subscriber.Models;
using static Essentials.RabbitMqClient.Metrics.MetricsRegistry;

namespace Essentials.RabbitMqClient.Metrics.Implementations;

/// <inheritdoc cref="IMetricsService" />
internal class MetricsService : IMetricsService
{
    private readonly IMetrics _metrics;

    private static readonly ConcurrentDictionary<SubscriptionKey, MetricTags> _subscriptionTags = new();
    private static readonly ConcurrentDictionary<PublishKey, MetricTags> _publishTags = new();
    
    public MetricsService(IMetrics metrics)
    {
        _metrics = metrics.CheckNotNull();
    }
    
    /// <inheritdoc cref="IMetricsService.StartHandleEventTimer" />
    public IDisposable StartHandleEventTimer(SubscriptionKey subscriptionKey)
    {
        var tags = _subscriptionTags.GetOrAdd(subscriptionKey, _ => GetMetricSubscriptionTags(subscriptionKey));

        return _metrics.Measure.Timer.Time(InEventsHandleTimer, tags);
    }

    /// <inheritdoc cref="IMetricsService.StartHandleEvent" />
    public void StartHandleEvent(SubscriptionKey subscriptionKey) =>
        IncrementSubscriptionCounter(subscriptionKey, InEventsCounterOptions);

    /// <inheritdoc cref="IMetricsService.SuccessHandleEvent" />
    public void SuccessHandleEvent(SubscriptionKey subscriptionKey) =>
        IncrementSubscriptionCounter(subscriptionKey, ValidInEventsCounterOptions);

    /// <inheritdoc cref="IMetricsService.ErrorHandleEvent" />
    public void ErrorHandleEvent(SubscriptionKey subscriptionKey) =>
        IncrementSubscriptionCounter(subscriptionKey, InvalidInEventsCounterOptions);
    
    /// <inheritdoc cref="IMetricsService.StartPublishEventTimer" />
    public IDisposable StartPublishEventTimer(PublishKey publishKey)
    {
        var tags = _publishTags.GetOrAdd(publishKey, _ => GetMetricPublishTags(publishKey));
        return _metrics.Measure.Timer.Time(OutEventsPublishTimer, tags);
    }

    /// <inheritdoc cref="IMetricsService.StartPublishEvent" />
    public void StartPublishEvent(PublishKey publishKey) =>
        IncrementPublishCounter(publishKey, OutEventsCounterOptions);

    /// <inheritdoc cref="IMetricsService.SuccessPublishEvent" />
    public void SuccessPublishEvent(PublishKey publishKey) =>
        IncrementPublishCounter(publishKey, ValidOutEventsCounterOptions);

    /// <inheritdoc cref="IMetricsService.ErrorPublishEvent" />
    public void ErrorPublishEvent(PublishKey publishKey) =>
        IncrementPublishCounter(publishKey, InvalidOutEventsCounterOptions);

    /// <summary>
    /// Инкрементирует счетчик количества обработанных сообщений
    /// </summary>
    /// <param name="subscriptionKey">Ключ подписки</param>
    /// <param name="counter">Счетчик</param>
    private void IncrementSubscriptionCounter(
        SubscriptionKey subscriptionKey,
        CounterOptions counter)
    {
        var tags = _subscriptionTags.GetOrAdd(subscriptionKey, _ => GetMetricSubscriptionTags(subscriptionKey));
        
        _metrics.Measure.Counter.Increment(counter, tags);
    }

    /// <summary>
    /// Инкрементирует счетчик количества отправленных сообщений
    /// </summary>
    /// <param name="publishKey">Ключ публикации события</param>
    /// <param name="counter">Счетчик</param>
    private void IncrementPublishCounter(
        PublishKey publishKey,
        CounterOptions counter)
    {
        var tags = _publishTags.GetOrAdd(publishKey, _ => GetMetricPublishTags(publishKey));
        
        _metrics.Measure.Counter.Increment(counter, tags);
    }

    /// <summary>
    /// Возвращает теги метрик для подписки на сообщение
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Теги</returns>
    private static MetricTags GetMetricSubscriptionTags(SubscriptionKey key) =>
        new(
            keys: ["queue_name", "routing_key"],
            values: [key.QueueKey.QueueName, key.RoutingKey.Key]);

    /// <summary>
    /// Возвращает теги метрик для публикации сообщения
    /// </summary>
    /// <param name="publishKey">Ключ публикации события</param>
    /// <returns>Теги</returns>
    private static MetricTags GetMetricPublishTags(PublishKey publishKey) =>
        new(
            keys: ["exchange_name", "routing_key"],
            values: [publishKey.Exchange, publishKey.RoutingKey?.Key ?? "undefined_key"]);
}