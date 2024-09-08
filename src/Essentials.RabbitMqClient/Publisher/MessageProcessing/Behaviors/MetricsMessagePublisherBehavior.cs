using Essentials.Utils.Extensions;
using Essentials.RabbitMqClient.Metrics;

namespace Essentials.RabbitMqClient.Publisher.MessageProcessing.Behaviors;

/// <summary>
/// Перехватчик отправки сообщения для сбора метрик
/// </summary>
public class MetricsMessagePublisherBehavior : IMessagePublishBehavior
{
    private readonly IMetricsService _metricsService;
    
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="metricsService">Сервис управления метриками</param>
    public MetricsMessagePublisherBehavior(IMetricsService metricsService)
    {
        _metricsService = metricsService.CheckNotNull();
    }
    
    /// <inheritdoc cref="IMessagePublishBehavior.Handle" />
    public async Task Handle(MessagePublishDelegate next)
    {
        var context = MessageContext.Current;
        context.CheckNotNull("Context must not be null here!");

        var publishKey = context.PublishKey;
        
        try
        {
            using var _ = _metricsService.StartPublishEventTimer(publishKey);
            _metricsService.StartPublishEvent(publishKey);

            await next();

            _metricsService.SuccessPublishEvent(publishKey);
        }
        catch
        {
            _metricsService.ErrorPublishEvent(publishKey);
            throw;
        }
    }
}