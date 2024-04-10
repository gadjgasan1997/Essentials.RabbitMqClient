using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Timer;

namespace Essentials.RabbitMqClient.Metrics;

/// <summary>
/// Реестр метрик
/// </summary>
internal static class MetricsRegistry
{
    /// <summary>
    /// Счетчик количества всех полученных сообщений
    /// </summary>
    public static CounterOptions InEventsCounterOptions => new()
    {
        Name = "RabbitMqClient In Events Counter"
    };

    /// <summary>
    /// Счетчик количества успешно обработанных сообщений
    /// </summary>
    public static CounterOptions ValidInEventsCounterOptions => new()
    {
        Name = "RabbitMqClient Valid In Events Counter"
    };

    /// <summary>
    /// Счетчик количества ошибочно обработанных сообщений
    /// </summary>
    public static CounterOptions InvalidInEventsCounterOptions => new()
    {
        Name = "RabbitMqClient Invalid In Events Counter"
    };
    
    /// <summary>
    /// Таймер времени обработки полученных сообщений
    /// </summary>
    public static TimerOptions InEventsHandleTimer => new()
    {
        Name = "RabbitMqClient In Events Handle Timer",
        MeasurementUnit = Unit.Events,
        DurationUnit = TimeUnit.Milliseconds,
        RateUnit = TimeUnit.Milliseconds
    };
    
    /// <summary>
    /// Счетчик количества всех отправленных сообщений
    /// </summary>
    public static CounterOptions OutEventsCounterOptions => new()
    {
        Name = "RabbitMqClient Out Events Counter"
    };

    /// <summary>
    /// Счетчик количества успешно отправленных сообщений
    /// </summary>
    public static CounterOptions ValidOutEventsCounterOptions => new()
    {
        Name = "RabbitMqClient Valid Out Events Counter"
    };

    /// <summary>
    /// Счетчик количества ошибочно отправленных сообщений
    /// </summary>
    public static CounterOptions InvalidOutEventsCounterOptions => new()
    {
        Name = "RabbitMqClient Invalid Out Events Counter"
    };
    
    /// <summary>
    /// Таймер времени отправки сообщений
    /// </summary>
    public static TimerOptions OutEventsPublishTimer => new()
    {
        Name = "RabbitMqClient Out Events Publish Timer",
        MeasurementUnit = Unit.Events,
        DurationUnit = TimeUnit.Milliseconds,
        RateUnit = TimeUnit.Milliseconds
    };
}