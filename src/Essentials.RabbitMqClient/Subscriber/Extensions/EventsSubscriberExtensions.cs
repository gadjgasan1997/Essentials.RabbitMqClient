using System.Reflection;
using Essentials.RabbitMqClient.Subscriber.Implementations;
using Essentials.RabbitMqClient.Subscriber.Models;
using Essentials.Utils.Extensions;
using Essentials.Utils.Reflection.Extensions;
using static Essentials.RabbitMqClient.Dictionaries.QueueLoggers;

namespace Essentials.RabbitMqClient.Subscriber.Extensions;

/// <summary>
/// Методы расширения для <see cref="IEventsSubscriber" />
/// </summary>
internal static class EventsSubscriberExtensions
{
    /// <summary>
    /// Подписывается на событие
    /// </summary>
    /// <param name="eventsSubscriber">Подписчик на события</param>
    /// <param name="subscriptionParams">Опции подписки на событие</param>
    /// <param name="assemblies">Список сборок</param>
    /// <param name="eventName">Название события</param>
    public static void SubscribeToEvent(
        this IEventsSubscriber eventsSubscriber,
        SubscriptionParams subscriptionParams,
        Assembly[] assemblies,
        string eventName)
    {
        try
        {
            // Получение типа события
            var eventType = assemblies.GetTypeByName(eventName, StringComparison.InvariantCultureIgnoreCase);

            // Получение и вызов метода
            var method = typeof(EventsSubscriber).GetMethod(nameof(EventsSubscriber.Subscribe)).CheckNotNull();

            var genericMethod = method.MakeGenericMethod(eventType);
            genericMethod.Invoke(eventsSubscriber, new object?[] { subscriptionParams });
        }
        catch (Exception ex)
        {
            MainLogger.Error(ex, $"Во время подписки на событие '{eventName}' произошла ошибка.");
        }
    }
}