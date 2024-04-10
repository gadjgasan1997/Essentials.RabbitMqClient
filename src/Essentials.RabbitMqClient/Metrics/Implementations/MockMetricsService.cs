using Essentials.RabbitMqClient.Publisher.Models;
using Essentials.RabbitMqClient.Subscriber.Models;

namespace Essentials.RabbitMqClient.Metrics.Implementations;

/// <inheritdoc cref="IMetricsService" />
internal class MockMetricsService : IMetricsService
{
    /// <inheritdoc cref="IMetricsService.StartHandleEventTimer" />
    public IDisposable? StartHandleEventTimer(SubscriptionKey subscriptionKey) => null;

    /// <inheritdoc cref="IMetricsService.StartHandleEvent" />
    public void StartHandleEvent(SubscriptionKey subscriptionKey) { }

    /// <inheritdoc cref="IMetricsService.SuccessHandleEvent" />
    public void SuccessHandleEvent(SubscriptionKey subscriptionKey) { }

    /// <inheritdoc cref="IMetricsService.ErrorHandleEvent" />
    public void ErrorHandleEvent(SubscriptionKey subscriptionKey) { }

    /// <inheritdoc cref="IMetricsService.StartPublishEventTimer" />
    public IDisposable? StartPublishEventTimer(PublishKey publishKey) => null;

    /// <inheritdoc cref="IMetricsService.StartPublishEvent" />
    public void StartPublishEvent(PublishKey publishKey) { }

    /// <inheritdoc cref="IMetricsService.SuccessPublishEvent" />
    public void SuccessPublishEvent(PublishKey publishKey) { }

    /// <inheritdoc cref="IMetricsService.ErrorPublishEvent" />
    public void ErrorPublishEvent(PublishKey publishKey) { }
}