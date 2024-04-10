using System.Diagnostics.CodeAnalysis;
using Essentials.RabbitMqClient.Models;
using Essentials.RabbitMqClient.Subscriber.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Essentials.RabbitMqClient.RabbitMqConnections;

/// <summary>
/// Фабрика для управления каналами RabbitMq
/// </summary>
internal interface IChannelFactory
{
    /// <summary>
    /// Возвращает или создает канал для публикации
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <returns>Канал</returns>
    IModel GetOrCreateChannelForPublish(ConnectionKey connectionKey);
   
    /// <summary>
    /// Возвращает или создает канал для подписки
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <param name="subscriptionKey">Ключ подписки</param>
    /// <param name="options">Опции подписки</param>
    /// <returns>Канал</returns>
    IModel GetOrCreateChannelForSubscribe(
        ConnectionKey connectionKey,
        SubscriptionKey subscriptionKey,
        SubscriptionOptions options);

    /// <summary>
    /// Создает слушателя
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <param name="subscriptionKey">Ключ подписки</param>
    /// <returns>Слушатель</returns>
    AsyncEventingBasicConsumer CreateConsumer(ConnectionKey connectionKey, SubscriptionKey subscriptionKey);

    /// <summary>
    /// Пытается вернуть ключи подписки на событие для слушателя
    /// </summary>
    /// <param name="consumerTags">Список тегов слушателя</param>
    /// <param name="routingKey">Ключ маршрутизации</param>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <param name="subscriptionKey">Ключ подписки</param>
    /// <returns></returns>
    bool TryGetSubscriptionKeys(
        string[] consumerTags,
        string routingKey,
        [NotNullWhen(true)] out ConnectionKey? connectionKey,
        [NotNullWhen(true)] out SubscriptionKey? subscriptionKey);
}