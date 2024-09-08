using System.Text;
using System.Diagnostics;
using System.Collections.Concurrent;
using Essentials.Utils.Extensions;
using Essentials.Logging.Extensions;
using Microsoft.Extensions.Logging;
using Essentials.RabbitMqClient.Models;
using Essentials.RabbitMqClient.Subscriber.Models;
using static System.DateTime;
using static System.Environment;
using static Essentials.Utils.Dictionaries.KnownDatesFormats;
using static Essentials.Serialization.Helpers.JsonHelpers;

namespace Essentials.RabbitMqClient.Subscriber.MessageProcessing.Behaviors;

/// <summary>
/// Перехватчик обработки сообщения для сбора логов
/// </summary>
public class LoggingMessageHandlerBehavior : IMessageHandlerBehavior
{
    private readonly ILoggerFactory _factory;
    private readonly ConcurrentDictionary<(ConnectionKey, SubscriptionKey), ILogger> _loggersMap = new();
    
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="factory"></param>
    public LoggingMessageHandlerBehavior(ILoggerFactory factory)
    {
        _factory = factory;
    }
    
    /// <inheritdoc cref="IMessageHandlerBehavior.Handle" />
    public async Task Handle(MessageHandlerDelegate next)
    {
        var context = MessageContext.Current;
        context.CheckNotNull("Context must not be null here!");
        
        var clock = new Stopwatch();
        clock.Start();
        
        var logger = GetLogger(context);
        
        using var _ = logger.BeginScope(
            new Dictionary<string, object?>
            {
                ["rabbit_message_id"] = Guid.NewGuid()
            });
        
        var eventArgs = context.EventArgs;
        var subscriptionKey = context.SubscriptionKey;
        
        try
        {
            logger
                .LogIfLevelIsTrace(() =>
                {
                    logger.LogTrace(
                        "Начинается обработка события с ключом '{key}'" +
                        "{newLine}Текущая дата/время: '{date}'" +
                        "{newLine}Параметры: '{eventArgs}'" +
                        "{newLine}Содержимое: '{content}'",
                        subscriptionKey,
                        NewLine, Now.ToString(LogDateLongFormat),
                        NewLine, Serialize(eventArgs),
                        NewLine, Encoding.UTF8.GetString(eventArgs.Body.Span));
                })
                .LogIfLevelIsDebug(() =>
                {
                    logger.LogDebug(
                        "Начинается обработка события с ключом '{key}'" +
                        "{newLine}Текущая дата/время: '{date}'" +
                        "{newLine}Параметры: {eventArgs}",
                        subscriptionKey,
                        NewLine, Now.ToString(LogDateLongFormat),
                        NewLine, Serialize(eventArgs));
                })
                .LogIfLevelIsInfo(() =>
                {
                    logger.LogInformation(
                        "Начинается обработка события с ключом '{key}'" +
                        "{newLine}Текущая дата/время: '{date}'",
                        subscriptionKey,
                        NewLine, Now.ToString(LogDateLongFormat));
                });
            
            await next();
            
            clock.Stop();

            logger.LogIfLevelIsInfoOrLow(() =>
            {
                logger.LogInformation(
                    "Событие с ключом '{key}' было обработано без исключений" +
                    "{newLine}Текущая дата/время: '{date}'. Затраченное время: '{elapsed}' мс",
                    subscriptionKey,
                    NewLine, Now.ToString(LogDateLongFormat), clock.ElapsedMilliseconds);
            });
        }
        catch (Exception exception)
        {
            clock.Stop();
            
            logger.LogError(
                exception,
                "Во время обработки сообщения с ключом '{key}' произошло исключение" +
                "{newLine}Текущая дата/время: '{date}'. Затраченное время: '{elapsed}' мс" +
                "{newLine}Параметры сообщения: {eventArgs}" +
                "{newLine}Сообщение: '{content}'",
                subscriptionKey,
                NewLine, Now.ToString(LogDateLongFormat), clock.ElapsedMilliseconds,
                NewLine, Serialize(eventArgs),
                NewLine, Encoding.UTF8.GetString(eventArgs.Body.Span));
            
            throw;
        }
    }

    /// <summary>
    /// Возвращает логгер
    /// </summary>
    /// <param name="context">Контекст обрабатываемого сообщения</param>
    /// <returns>Логгер</returns>
    private ILogger GetLogger(MessageContext.Context context)
    {
        var connectionKey = context.ConnectionKey;
        var subscriptionKey = context.SubscriptionKey;

        return _loggersMap.GetOrAdd(
            (connectionKey, subscriptionKey),
            _ =>
            {
                var loggerName = GetLoggerName(context);
                return _factory.CreateLogger(loggerName);
            });
    }
    
    /// <summary>
    /// Возвращает название логгера
    /// </summary>
    /// <param name="context">Контекст сообщения</param>
    /// <returns>Название логгера</returns>
    protected virtual string GetLoggerName(MessageContext.Context context)
    {
        var connectionKey = context.ConnectionKey;
        var subscriptionKey = context.SubscriptionKey;

        return $"Essentials.RabbitMqClient.HandleMessageBehavior." +
               $"{connectionKey.ConnectionName}." +
               $"{subscriptionKey.RoutingKey.Key}";
    }
}