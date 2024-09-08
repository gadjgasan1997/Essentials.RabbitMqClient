using Essentials.Utils.Extensions;
using Essentials.RabbitMqClient.Options;
using Essentials.RabbitMqClient.Subscriber.MessageProcessing;
using static Essentials.RabbitMqClient.Dictionaries.MessageContentType;

namespace Essentials.RabbitMqClient.Configuration.Builders.Abstractions;

/// <summary>
/// Билдер подписки на событие
/// </summary>
/// <typeparam name="TEvent">Тип события</typeparam>
public abstract class BaseSubscriptionBuilder<TEvent>
    where TEvent : IEvent
{
    private readonly string _queueName;
    private readonly string _routingKey;
    private string? _contentType;
    private ushort? _prefetchCount;
    private readonly List<BehaviorInfo> _behaviors = [];
    
    private protected BaseSubscriptionBuilder(string queueName, string routingKey)
    {
        _queueName = queueName.CheckNotNullOrEmpty("Название очереди для конфигурации не может быть пустым");
        _routingKey = routingKey.CheckNotNullOrEmpty("Ключ маршрутизации для конфигурации не может быть пустым");
    }
    
    /// <summary>
    /// Устанавливает тип содержимого для сообщения <see cref="JSON" />
    /// </summary>
    /// <returns>Билдер подписки на событие</returns>
    public BaseSubscriptionBuilder<TEvent> WithJsonContentType()
    {
        _contentType = JSON;
        return this;
    }
    
    /// <summary>
    /// Устанавливает тип содержимого для сообщения <see cref="XML" />
    /// </summary>
    /// <returns>Билдер подписки на событие</returns>
    public BaseSubscriptionBuilder<TEvent> WithXmlContentType()
    {
        _contentType = XML;
        return this;
    }
    
    /// <summary>
    /// Устанавливает количество сообщений, которое может забрать подписчик
    /// </summary>
    /// <param name="prefetchCount">Количество сообщений</param>
    /// <returns>Билдер подписки на событие</returns>
    public BaseSubscriptionBuilder<TEvent> SetPrefetchCount(ushort prefetchCount)
    {
        _prefetchCount = prefetchCount;
        return this;
    }
    
    /// <summary>
    /// Добавляет перехватчик обработки сообщения
    /// </summary>
    /// <typeparam name="TBehavior">Тип перехватчик обработки сообщения</typeparam>
    /// <returns></returns>
    public BaseSubscriptionBuilder<TEvent> AttachBehavior<TBehavior>()
        where TBehavior : IMessageHandlerBehavior
    {
        _behaviors.Add(new BehaviorInfo
        {
            Name = typeof(TBehavior).FullName!,
        });
        
        return this;
    }
    
    /// <summary>
    /// Билдит опции подписки на событие
    /// </summary>
    /// <returns>Опции подписки на событие</returns>
    private protected SubscriptionOptions Build()
    {
        return new SubscriptionOptions
        {
            Key = new SubscriptionOptions.KeyElement
            {
                QueueName = _queueName,
                RoutingKey = _routingKey
            },
            Options = new SubscriptionOptions.ValueElement
            {
                ContentType = _contentType,
                PrefetchCount = _prefetchCount,
                Behaviors = _behaviors,
                NeedConsume = true
            }
        };
    }
}