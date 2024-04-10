using Essentials.RabbitMqClient.Publisher.Models;
using Essentials.RabbitMqClient.Subscriber.Models;

namespace Essentials.RabbitMqClient.Metrics;

/// <summary>
/// Сервис управления метриками
/// </summary>
internal interface IMetricsService
{
    /// <summary>
    /// Запускает таймер обработки события
    /// </summary>
    /// <param name="subscriptionKey">Ключ подписки</param>
    /// <returns></returns>
    IDisposable? StartHandleEventTimer(SubscriptionKey subscriptionKey);
    
    /// <summary>
    /// Инкрементирует счетчик количества всех полученных сообщений
    /// </summary>
    /// <param name="subscriptionKey">Ключ подписки</param>
    void StartHandleEvent(SubscriptionKey subscriptionKey);
    
    /// <summary>
    /// Инкрементирует счетчик количества успешно обработанных сообщений
    /// </summary>
    /// <param name="subscriptionKey">Ключ подписки</param>
    void SuccessHandleEvent(SubscriptionKey subscriptionKey);
    
    /// <summary>
    /// Инкрементирует счетчик количества ошибочно обработанных сообщений
    /// </summary>
    /// <param name="subscriptionKey">Ключ подписки</param>
    void ErrorHandleEvent(SubscriptionKey subscriptionKey);
    
    /// <summary>
    /// Запускает таймер отправки сообщения
    /// </summary>
    /// <param name="publishKey">Ключ публикации события</param>
    /// <returns></returns>
    IDisposable? StartPublishEventTimer(PublishKey publishKey);
    
    /// <summary>
    /// Инкрементирует счетчик количества всех отправленных сообщений
    /// </summary>
    /// <param name="publishKey">Ключ публикации события</param>
    void StartPublishEvent(PublishKey publishKey);
    
    /// <summary>
    /// Инкрементирует счетчик количества успешно отправленных сообщений
    /// </summary>
    /// <param name="publishKey">Ключ публикации события</param>
    void SuccessPublishEvent(PublishKey publishKey);
    
    /// <summary>
    /// Инкрементирует счетчик количества ошибочно отправленных сообщений
    /// </summary>
    /// <param name="publishKey">Ключ публикации события</param>
    void ErrorPublishEvent(PublishKey publishKey);
}