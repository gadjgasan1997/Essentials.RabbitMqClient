using Essentials.RabbitMqClient.Models;
using Essentials.RabbitMqClient.Subscriber.Models;

namespace Essentials.RabbitMqClient.Subscriber;

/// <summary>
/// Провайдер для получения опций
/// </summary>
internal interface IOptionsProvider
{
    /// <summary>
    /// Добавляет опции соединения
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <param name="subscriptionsOptionsMap">Мапа с опциями подписки на события</param>
    void AddConnectionOptions(
        ConnectionKey connectionKey,
        IReadOnlyDictionary<SubscriptionKey, SubscriptionOptions> subscriptionsOptionsMap);
    
    /// <summary>
    /// Возвращает опции подписки на событие
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <param name="subscriptionKey">Ключ подписки</param>
    /// <returns>Опции подписки</returns>
    SubscriptionOptions GetSubscriptionOptions(ConnectionKey connectionKey, SubscriptionKey subscriptionKey);
    
    /// <summary>
    /// Возвращает опции регистрации обработчиков
    /// </summary>
    /// <returns></returns>
    IEnumerable<RegisterEventsHandlersOptions> GetOptionsForRegisterEventsHandlers();
    
    /// <summary>
    /// Возвращает информацию по существующим подпискам на события
    /// </summary>
    /// <returns></returns>
    IReadOnlyDictionary<ConnectionKey, IEnumerable<SubscriptionInfo>> GetSubscriptionsInfo();
}