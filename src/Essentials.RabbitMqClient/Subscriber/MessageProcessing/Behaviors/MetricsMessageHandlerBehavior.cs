using Essentials.Utils.Extensions;
using Essentials.RabbitMqClient.Metrics;

namespace Essentials.RabbitMqClient.Subscriber.MessageProcessing.Behaviors;

/// <summary>
/// Перехватчик обработки сообщения для сбора метрик
/// </summary>
public class MetricsMessageHandlerBehavior : IMessageHandlerBehavior
{
    private readonly IMetricsService _metricsService;
    
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="metricsService">Сервис управления метриками</param>
    public MetricsMessageHandlerBehavior(IMetricsService metricsService)
    {
        _metricsService = metricsService.CheckNotNull();
    }
    
    /// <inheritdoc cref="IMessageHandlerBehavior.Handle" />
    public async Task Handle(MessageHandlerDelegate next)
    {
        var context = MessageContext.Current;
        context.CheckNotNull("Context must not be null here!");
        
        var subscriptionKey = context.SubscriptionKey;
        
        using var _ = _metricsService.StartHandleEventTimer(subscriptionKey);
        _metricsService.StartHandleEvent(subscriptionKey);

        try
        {
            await next();

            _metricsService.SuccessHandleEvent(subscriptionKey);
        }
        catch
        {
            _metricsService.ErrorHandleEvent(subscriptionKey);
            throw;
        }
    }
}