using System.Reflection;
using Essentials.Utils.Reflection.Extensions;
using Essentials.RabbitMqClient.Dictionaries;
using Essentials.RabbitMqClient.Exceptions;
using Essentials.RabbitMqClient.Extensions;
using Essentials.RabbitMqClient.Models;
using Essentials.RabbitMqClient.Options;
using Essentials.RabbitMqClient.Subscriber.Implementations;
using Essentials.RabbitMqClient.Subscriber.Models;
using static System.Environment;
using static Essentials.Serialization.Helpers.JsonHelpers;
using static Essentials.RabbitMqClient.Dictionaries.QueueLoggers;
using SubscriptionOptions = Essentials.RabbitMqClient.Subscriber.Models.SubscriptionOptions;

namespace Essentials.RabbitMqClient.Subscriber;

/// <summary>
/// Билдер для <see cref="IOptionsProvider" />
/// </summary>
internal static class OptionsProviderBuilder
{
    /// <summary>
    /// Билдит провайдер получения опций
    /// </summary>
    /// <param name="connectionsOptions"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static OptionsProvider Build(
        IReadOnlyDictionary<string, ModelOptions> connectionsOptions,
        Assembly[] assemblies)
    {
        var provider = new OptionsProvider();

        foreach (var (connectionName, modelOptions) in connectionsOptions)
        {
            AddConnectionOptionsToProvider(
                provider,
                connectionName,
                modelOptions.SubscriptionsOptions,
                assemblies);
        }

        return provider;
    }
    
    /// <summary>
    /// Добавляет опции соединения в провайдер
    /// </summary>
    /// <param name="provider">Провайдер</param>
    /// <param name="connectionName"></param>
    /// <param name="subscriptionOptions"></param>
    /// <param name="assemblies">Сборки</param>
    private static void AddConnectionOptionsToProvider(
        OptionsProvider provider,
        string connectionName,
        List<Essentials.RabbitMqClient.Options.SubscriptionOptions> subscriptionOptions,
        Assembly[] assemblies)
    {
        var subscriptionsOptionsMap = GetSubscriptionsOptions(connectionName, subscriptionOptions, assemblies);
        
        var connectionKey = ConnectionKey.New(connectionName);
        provider.AddConnectionOptions(connectionKey, subscriptionsOptionsMap);
    }
    
    /// <summary>
    /// Возвращает опции подписки на события
    /// </summary>
    /// <param name="connectionName"></param>
    /// <param name="subscriptionOptions"></param>
    /// <param name="assemblies">Список сборок</param>
    private static Dictionary<SubscriptionKey, SubscriptionOptions> GetSubscriptionsOptions(
        string connectionName,
        List<Essentials.RabbitMqClient.Options.SubscriptionOptions> subscriptionOptions,
        Assembly[] assemblies)
    {
        var subscriptionsOptionsMap = new Dictionary<SubscriptionKey, SubscriptionOptions>();
        if (subscriptionOptions.Count is 0)
        {
            MainLogger.Warn($"Не указаны опции подписки на события для соединения '{connectionName}'.");
            return subscriptionsOptionsMap;
        }

        foreach (var options in subscriptionOptions)
        {
            if (!options.Key.CheckRequiredProperties(out var keyEmptyProperties))
            {
                throw new InvalidConfigurationException(
                    $"В конфигурации соединения '{connectionName}' не заполнены обязательные свойства подписки на событие. " +
                    $"Свойства, требующие заполнения: '{keyEmptyProperties.GetNames()}'");
            }
            
            if (!options.Options.CheckRequiredProperties(out var optionsEmptyProperties))
            {
                throw new InvalidConfigurationException(
                    $"В конфигурации соединения '{connectionName}' не заполнены обязательные свойства подписки на событие. " +
                    $"Свойства, требующие заполнения: '{optionsEmptyProperties.GetNames()}'");
            }
            
            if (options.Options.NeedConsume.HasValue && !options.Options.NeedConsume.Value)
            {
                MainLogger.Warn(
                    $"Сообщения из очереди '{options.Key.QueueName}' " +
                    $"с ключом маршрутизации '{options.Key.RoutingKey}' не будут обрабатываться");
                
                continue;
            }
            
            // Проверка на дублирование
            var queueKey = QueueKey.New(options.Key.QueueName);
            var routingKey = RoutingKey.New(options.Key.RoutingKey);
            var subscriptionKey = SubscriptionKey.New(queueKey, routingKey);
            
            if (subscriptionsOptionsMap.TryGetValue(subscriptionKey, out var existsSubscriptionOptions))
            {
                throw new InvalidConfigurationException(
                    $"Для события с ключом '{subscriptionKey}' уже настроена подписка." +
                    $"{NewLine}Опции существующей подписки: '{Serialize(existsSubscriptionOptions)}'.");
            }
            
            // Добавление подписки на событие
            var contentType = options.Options.ContentType ?? MessageContentType.JSON;
            var prefetchCount = options.Options.PrefetchCount ?? 5;

            var behaviors = GetBehaviors(assemblies, options.Options.Behaviors).Distinct().ToList();

            var newSubscriptionOptions = new SubscriptionOptions(
                options.Options.TypeName,
                options.Options.HandlerTypeName,
                contentType,
                prefetchCount,
                options.Options.Correlation ?? false,
                behaviors);
            
            subscriptionsOptionsMap.Add(subscriptionKey, newSubscriptionOptions);
        }

        return subscriptionsOptionsMap;
    }

    /// <summary>
    /// Возвращает список перехватчиков запросов
    /// </summary>
    /// <param name="assemblies">Список сборок</param>
    /// <param name="behaviorsInfo">Типы перехватчиков запросов</param>
    /// <returns></returns>
    private static IEnumerable<Type> GetBehaviors(
        Assembly[] assemblies,
        IEnumerable<BehaviorInfo> behaviorsInfo)
    {
        var behaviors = behaviorsInfo.Where(info => !string.IsNullOrWhiteSpace(info.Name));
        foreach (var behavior in behaviors)
            yield return GetBehaviorType(assemblies, behavior.Name);
    }

    /// <summary>
    /// Возвращает тип перехватчика запроса
    /// </summary>
    /// <param name="assemblies">Список сборок</param>
    /// <param name="behaviorTypeName">Название типа перехватчика</param>
    /// <returns></returns>
    /// <exception cref="InvalidConfigurationException"></exception>
    private static Type GetBehaviorType(Assembly[] assemblies, string behaviorTypeName)
    {
        try
        {
            return assemblies.GetTypeByName(behaviorTypeName, StringComparison.InvariantCultureIgnoreCase);
        }
        catch (Exception exception)
        {
            throw new InvalidConfigurationException(
                $"Ошибка получения перехватчика запроса с типом '{behaviorTypeName}'",
                exception);
        }
    }
}