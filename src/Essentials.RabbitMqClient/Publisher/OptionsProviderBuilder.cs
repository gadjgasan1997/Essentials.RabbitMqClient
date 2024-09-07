using System.Reflection;
using Essentials.Utils.Reflection.Extensions;
using Essentials.RabbitMqClient.Dictionaries;
using Essentials.RabbitMqClient.Exceptions;
using Essentials.RabbitMqClient.Extensions;
using Essentials.RabbitMqClient.Models;
using Essentials.RabbitMqClient.Options;
using Essentials.RabbitMqClient.Publisher.Implementations;
using Essentials.RabbitMqClient.Publisher.Models;
using PublishOptions = Essentials.RabbitMqClient.Publisher.Models.PublishOptions;
using RpcRequestOptions = Essentials.RabbitMqClient.Publisher.Models.RpcRequestOptions;
using static System.Environment;
using static Essentials.Serialization.Helpers.JsonHelpers;
using static Essentials.RabbitMqClient.Dictionaries.QueueLoggers;

namespace Essentials.RabbitMqClient.Publisher;

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
                modelOptions.PublishOptions,
                modelOptions.RpcRequestsOptions,
                assemblies);
        }

        return provider;
    }

    /// <summary>
    /// Добавляет опции соединения в провайдер
    /// </summary>
    /// <param name="provider">Провайдер</param>
    /// <param name="connectionName"></param>
    /// <param name="publishOptions"></param>
    /// <param name="rpcRequestsOptions"></param>
    /// <param name="assemblies">Сборки</param>
    private static void AddConnectionOptionsToProvider(
        OptionsProvider provider,
        string connectionName,
        List<Essentials.RabbitMqClient.Options.PublishOptions> publishOptions,
        List<Essentials.RabbitMqClient.Options.RpcRequestOptions> rpcRequestsOptions,
        Assembly[] assemblies)
    {
        var connectionKey = ConnectionKey.New(connectionName);
        var publishOptionsMap = GetPublishOptions(connectionName, publishOptions, assemblies);
        var rpcRequestsOptionsMap = GetRpcRequestsOptions(connectionName, rpcRequestsOptions, assemblies);
        
        provider.AddConnectionOptions(connectionKey, publishOptionsMap, rpcRequestsOptionsMap);
    }

    /// <summary>
    /// Возвращает опции публикации событий
    /// </summary>
    /// <param name="connectionName"></param>
    /// <param name="publishOptions"></param>
    /// <param name="assemblies">Список сборок</param>
    private static Dictionary<PublishKey, PublishOptions> GetPublishOptions(
        string connectionName,
        List<Essentials.RabbitMqClient.Options.PublishOptions> publishOptions,
        Assembly[] assemblies)
    {
        var publishOptionsMap = new Dictionary<PublishKey, PublishOptions>();
        
        if (publishOptions.Count is 0)
        {
            MainLogger.Info($"Не указаны опции публикации сообщений для соединения '{connectionName}'.");
            return publishOptionsMap;
        }

        foreach (var options in publishOptions)
        {
            if (!options.Key.CheckRequiredProperties(out var keyEmptyProperties))
            {
                throw new InvalidConfigurationException(
                    $"В конфигурации соединения '{connectionName}' не заполнены обязательные свойства публикации события. " +
                    $"Свойства, требующие заполнения: '{keyEmptyProperties.GetNames()}'");
            }
            
            if (!options.Options.CheckRequiredProperties(out var optionsEmptyProperties))
            {
                throw new InvalidConfigurationException(
                    $"В конфигурации соединения '{connectionName}' не заполнены обязательные свойства публикации события. " +
                    $"Свойства, требующие заполнения: '{optionsEmptyProperties.GetNames()}'");
            }
            
            // Проверка на дублирование
            var eventKey = EventKey.New(options.Key.TypeName);
            
            var publishKey = string.IsNullOrWhiteSpace(options.Key.RoutingKey)
                ? PublishKey.New(options.Key.Exchange, eventKey)
                : PublishKey.New(options.Key.Exchange, eventKey, RoutingKey.New(options.Key.RoutingKey));
            
            if (publishOptionsMap.TryGetValue(publishKey, out var existsPublishOptions))
            {
                throw new InvalidConfigurationException(
                    $"Для события с ключом '{publishKey}' уже настроена публикация." +
                    $"{NewLine}Опции существующей публикации: '{Serialize(existsPublishOptions)}'.");
            }
            
            var contentType = string.IsNullOrWhiteSpace(options.Options.ContentType)
                ? MessageContentType.JSON
                : options.Options.ContentType;

            var behaviors = GetBehaviors(assemblies, options.Options.Behaviors).Distinct().ToList();

            var newPublishOptions = new PublishOptions(
                contentType,
                options.Options.RetryCount ?? 5,
                options.Options.DeliveryMode ?? 2,
                behaviors);
            
            publishOptionsMap.Add(publishKey, newPublishOptions);
        }

        return publishOptionsMap;
    }

    /// <summary>
    /// Возвращает опции Rpc запросов
    /// </summary>
    /// <param name="connectionName">Название соединения</param>
    /// <param name="rpcRequestsOptions"></param>
    /// <param name="assemblies">Список сборок</param>
    private static Dictionary<PublishKey, RpcRequestOptions> GetRpcRequestsOptions(
        string connectionName,
        List<Essentials.RabbitMqClient.Options.RpcRequestOptions> rpcRequestsOptions,
        Assembly[] assemblies)
    {
        var rpcRequestsOptionsMap = new Dictionary<PublishKey, RpcRequestOptions>();
        if (rpcRequestsOptions.Count is 0)
        {
            MainLogger.Info($"Не указаны опции Rpc запросов для соединения '{connectionName}'.");
            return rpcRequestsOptionsMap;
        }

        foreach (var options in rpcRequestsOptions)
        {
            if (!options.Key.CheckRequiredProperties(out var keyEmptyProperties))
            {
                throw new InvalidConfigurationException(
                    $"В конфигурации соединения '{connectionName}' " +
                    $"не заполнены обязательные свойства Rpc запросов. " +
                    $"Свойства, требующие заполнения: '{keyEmptyProperties.GetNames()}'");
            }
            
            if (!options.Options.CheckRequiredProperties(out var optionsEmptyProperties))
            {
                throw new InvalidConfigurationException(
                    $"В конфигурации соединения '{connectionName}' " +
                    $"не заполнены обязательные свойства Rpc запросов. " +
                    $"Свойства, требующие заполнения: '{optionsEmptyProperties.GetNames()}'");
            }
            
            // Проверка на дублирование
            var eventKey = EventKey.New(options.Key.TypeName);
            var routingKey = RoutingKey.New(options.Key.RoutingKey);
            var publishKey = PublishKey.New(options.Key.Exchange, eventKey, routingKey);
            
            if (rpcRequestsOptionsMap.TryGetValue(publishKey, out var requestOptions))
            {
                throw new InvalidConfigurationException(
                    $"Для события с ключом '{publishKey}' уже настроены опции Rpc запросов." +
                    $"{NewLine}Опции существующего запроса: '{Serialize(requestOptions)}'.");
            }
            
            var replyTo = RoutingKey.New(options.Options.ReplyTo);

            var timeout = TimeSpan.FromSeconds(options.Options.Timeout);
            
            var contentType = string.IsNullOrWhiteSpace(options.Options.ContentType)
                ? MessageContentType.JSON
                : options.Options.ContentType;

            var behaviors = GetBehaviors(assemblies, options.Options.Behaviors).Distinct().ToList();

            var rpcRequestOptions = new RpcRequestOptions(
                contentType,
                options.Options.RetryCount ?? 5,
                options.Options.DeliveryMode ?? 2,
                replyTo,
                timeout,
                behaviors);
            
            rpcRequestsOptionsMap.Add(publishKey, rpcRequestOptions);
        }

        return rpcRequestsOptionsMap;
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