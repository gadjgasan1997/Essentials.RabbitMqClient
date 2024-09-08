using Essentials.Utils.Extensions;
using Essentials.RabbitMqClient.Options;
using Essentials.RabbitMqClient.Publisher.MessageProcessing;
using static Essentials.RabbitMqClient.Dictionaries.MessageContentType;

namespace Essentials.RabbitMqClient.Configuration.Builders;

/// <summary>
/// Билдер публикации события
/// </summary>
/// <typeparam name="TEvent">Тип события</typeparam>
public class PublishBuilder<TEvent>
    where TEvent : IEvent
{
    private protected readonly string ExchangeName;
    private protected readonly string? RoutingKey;
    
    private protected string? ContentType;
    private protected int? RetryCount;
    private protected byte? DeliveryMode;
    private protected readonly List<BehaviorInfo> Behaviors = [];
    
    internal PublishBuilder(string exchangeName, string? routingKey = null)
    {
        ExchangeName = exchangeName.CheckNotNullOrEmpty("Название обменника для конфигурации не может быть пустым");
        RoutingKey = routingKey;
    }
    
    /// <summary>
    /// Устанавливает тип содержимого для сообщения <see cref="JSON" />
    /// </summary>
    /// <returns>Билдер публикации события</returns>
    public PublishBuilder<TEvent> WithJsonContentType()
    {
        ContentType = JSON;
        return this;
    }
    
    /// <summary>
    /// Устанавливает тип содержимого для сообщения <see cref="XML" />
    /// </summary>
    /// <returns>Билдер публикации события</returns>
    public PublishBuilder<TEvent> WithXmlContentType()
    {
        ContentType = XML;
        return this;
    }
    
    /// <summary>
    /// Устанавливает количество попыток публикации сообщения
    /// </summary>
    /// <param name="retryCount">Количество попыток</param>
    /// <returns>Билдер публикации события</returns>
    public PublishBuilder<TEvent> SetRetryCount(int retryCount)
    {
        RetryCount = retryCount;
        return this;
    }
    
    /// <summary>
    /// Устанавливает режим доставки сообщения
    /// </summary>
    /// <param name="deliveryMode">Режим доставки сообщения</param>
    /// <returns>Билдер публикации события</returns>
    public PublishBuilder<TEvent> SetDeliveryMode(byte deliveryMode)
    {
        DeliveryMode = deliveryMode;
        return this;
    }
    
    /// <summary>
    /// Добавляет перехватчик обработки сообщения
    /// </summary>
    /// <typeparam name="TBehavior">Тип перехватчик обработки сообщения</typeparam>
    /// <returns></returns>
    public PublishBuilder<TEvent> AttachBehavior<TBehavior>()
        where TBehavior : IMessagePublishBehavior
    {
        Behaviors.Add(new BehaviorInfo
        {
            Name = typeof(TBehavior).FullName!,
        });
        
        return this;
    }

    /// <summary>
    /// Билдит опции публикации события
    /// </summary>
    /// <returns>Опции публикации события</returns>
    internal PublishOptions Build()
    {
        return new PublishOptions
        {
            Key = new PublishOptions.KeyElement
            {
                Exchange = ExchangeName,
                RoutingKey = RoutingKey,
                TypeName = typeof(TEvent).FullName!
            },
            Options = new PublishOptions.ValueElement
            {
                ContentType = ContentType,
                RetryCount = RetryCount,
                DeliveryMode = DeliveryMode,
                Behaviors = Behaviors
            }
        };
    }
}