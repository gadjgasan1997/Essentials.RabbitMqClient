using System.Reflection;
using Essentials.Utils.Reflection.Extensions;
using Essentials.RabbitMqClient.Dictionaries;
using Essentials.RabbitMqClient.Exceptions;
using Essentials.RabbitMqClient.Extensions;
using Essentials.RabbitMqClient.Models;
using Essentials.RabbitMqClient.Options;
using Essentials.RabbitMqClient.Publisher.Models;
using Essentials.RabbitMqClient.Subscriber.Models;
using SubscriptionOptions = Essentials.RabbitMqClient.Subscriber.Models.SubscriptionOptions;
using PublishOptions = Essentials.RabbitMqClient.Publisher.Models.PublishOptions;
using static System.Environment;
using static Essentials.Utils.Reflection.Helpers.AssemblyHelpers;
using static Essentials.Serialization.Helpers.JsonHelpers;
using static Essentials.RabbitMqClient.Dictionaries.QueueLoggers;

namespace Essentials.RabbitMqClient.OptionsProvider;

/// <summary>
/// Билдер для <see cref="IOptionsProvider" />
/// </summary>
internal class Builder
{
    private readonly List<Queue> _queuesForDeclare = new();
    private readonly Dictionary<SubscriptionKey, SubscriptionOptions> _subscriptionsOptionsMap = new();
    
    private readonly List<Exchange> _exchangesForDeclare = new();
    private readonly Dictionary<PublishKey, PublishOptions> _publishOptionsMap = new();
    
    private readonly Dictionary<PublishKey, RpcRequestOptions> _rpcRequestsOptionsMap = new();
    
    /// <summary>
    /// Добавляет опции соединения в провайдер
    /// </summary>
    /// <param name="provider">Провайдер</param>
    /// <param name="options">Опции соединения</param>
    public void AddConnectionOptionsToProvider(IOptionsProvider provider, ConnectionOptions options)
    {
        var assemblies = GetAllAssembliesWithReferences().ToArray();

        FillQueuesList(options);
        FillSubscriptionsOptions(options, assemblies);
        
        FillExchangesList(options);
        FillPublishOptions(options, assemblies);
        
        FillRpcRequestsOptions(options, assemblies);
        
        var connectionKey = ConnectionKey.New(options.Name);
        
        provider.AddConnectionOptions(
            connectionKey,
            _queuesForDeclare,
            _exchangesForDeclare,
            _subscriptionsOptionsMap,
            _publishOptionsMap,
            _rpcRequestsOptionsMap);
    }
    
    /// <summary>
    /// Заполняет список очередей
    /// </summary>
    /// <param name="connectionOptions"></param>
    private void FillQueuesList(ConnectionOptions connectionOptions)
    {
        if (connectionOptions.Queues.Count is 0)
        {
            MainLogger.Info($"Не указаны опции очередей для соединения '{connectionOptions.Name}'.");
            return;
        }
        
        foreach (var options in connectionOptions.Queues)
        {
            if (!options.CheckRequiredProperties(out var queueEmptyProperties))
            {
                throw new InvalidConfigurationException(
                    "В конфигурации не заполнены обязательные свойства очереди. " +
                    $"Свойства, требующие заполнения: '{queueEmptyProperties.GetNames()}'");
            }

            if (!options.CheckBindingsOptionsForEmpty(out var bindingsEmptyProperties))
            {
                throw new InvalidConfigurationException(
                    $"В конфигурации не заполнены обязательные свойства привязки для очереди '{options.Name}'. " +
                    $"Свойства, требующие заполнения: '{bindingsEmptyProperties.GetNames()}'");
            }
            
            var queueKey = QueueKey.New(options.Name);
            if (_queuesForDeclare.Select(q => q.QueueKey).Contains(queueKey))
            {
                throw new InvalidConfigurationException(
                    $"Конфигурация для очереди с ключом '{queueKey}' задублирована.");
            }

            var queueBindings = new List<Binding>();
            foreach (var binding in options.Bindings)
            {
                var routingKey = RoutingKey.New(binding.RoutingKey);
                queueBindings.Add(new Binding(binding.Exchange, routingKey));
            }
            
            var queue = new Queue(
                queueKey,
                options.Durable ?? true,
                options.Exclusive ?? false,
                options.AutoDelete ?? false,
                queueBindings);

            _queuesForDeclare.Add(queue);
        }
    }

    /// <summary>
    /// Заполняет опции подписки на события
    /// </summary>
    /// <param name="connectionOptions">Опции соединения</param>
    /// <param name="assemblies">Список сборок</param>
    private void FillSubscriptionsOptions(
        ConnectionOptions connectionOptions,
        Assembly[] assemblies)
    {
        if (connectionOptions.SubscriptionsOptions.Count is 0)
        {
            MainLogger.Warn($"Не указаны опции подписки на события для соединения '{connectionOptions.Name}'.");
            return;
        }

        foreach (var options in connectionOptions.SubscriptionsOptions)
        {
            if (!options.Key.CheckRequiredProperties(out var keyEmptyProperties))
            {
                throw new InvalidConfigurationException(
                    $"В конфигурации соединения '{connectionOptions.Name}' не заполнены обязательные свойства подписки на событие. " +
                    $"Свойства, требующие заполнения: '{keyEmptyProperties.GetNames()}'");
            }
            
            if (!options.Options.CheckRequiredProperties(out var optionsEmptyProperties))
            {
                throw new InvalidConfigurationException(
                    $"В конфигурации соединения '{connectionOptions.Name}' не заполнены обязательные свойства подписки на событие. " +
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
            
            if (_subscriptionsOptionsMap.TryGetValue(subscriptionKey, out var existsSubscriptionOptions))
            {
                throw new InvalidConfigurationException(
                    $"Для события с ключом '{subscriptionKey}' уже настроена подписка." +
                    $"{NewLine}Опции существующей подписки: '{Serialize(existsSubscriptionOptions)}'.");
            }
            
            // Добавление подписки на событие
            var contentType = options.Options.ContentType ?? MessageContentType.JSON;
            var prefetchCount = options.Options.PrefetchCount ?? 5;

            var behaviors = GetBehaviors(assemblies, options.Options.Behaviors).Distinct().ToList();

            var subscriptionOptions = new SubscriptionOptions(
                options.Options.TypeName,
                options.Options.ResponseTypeName,
                options.Options.HandlerTypeName,
                contentType,
                prefetchCount,
                options.Options.Correlation ?? false,
                behaviors);
            
            _subscriptionsOptionsMap.Add(subscriptionKey, subscriptionOptions);
        }
    }

    /// <summary>
    /// Заполняет список обменников
    /// </summary>
    /// <param name="connectionOptions">Опции соединения</param>
    private void FillExchangesList(ConnectionOptions connectionOptions)
    {
        if (connectionOptions.Exchanges.Count is 0)
        {
            MainLogger.Info($"Не указаны опции обменников для соединения '{connectionOptions.Name}'.");
            return;
        }

        foreach (var exchange in connectionOptions.Exchanges)
        {
            if (!exchange.CheckRequiredProperties(out var exchangeEmptyProperties))
            {
                throw new InvalidConfigurationException(
                    "В конфигурации не заполнены обязательные свойства обменника. " +
                    $"Свойства, требующие заполнения: '{exchangeEmptyProperties.GetNames()}'");
            }
            
            if (_exchangesForDeclare.Any(ex => ex.Name == exchange.Name))
                continue;
            
            var exchangeForDeclare = new Exchange(
                exchange.Name,
                exchange.Type,
                exchange.Durable ?? true,
                exchange.AutoDelete ?? false);
            
            _exchangesForDeclare.Add(exchangeForDeclare);
        }
    }

    /// <summary>
    /// Заполняет опции публикации событий
    /// </summary>
    /// <param name="connectionOptions">Опции соединения</param>
    /// <param name="assemblies">Список сборок</param>
    private void FillPublishOptions(
        ConnectionOptions connectionOptions,
        Assembly[] assemblies)
    {
        if (connectionOptions.PublishOptions.Count is 0)
        {
            MainLogger.Info($"Не указаны опции публикации сообщений для соединения '{connectionOptions.Name}'.");
            return;
        }

        foreach (var options in connectionOptions.PublishOptions)
        {
            if (!options.Key.CheckRequiredProperties(out var keyEmptyProperties))
            {
                throw new InvalidConfigurationException(
                    $"В конфигурации соединения '{connectionOptions.Name}' не заполнены обязательные свойства публикации события. " +
                    $"Свойства, требующие заполнения: '{keyEmptyProperties.GetNames()}'");
            }
            
            if (!options.Options.CheckRequiredProperties(out var optionsEmptyProperties))
            {
                throw new InvalidConfigurationException(
                    $"В конфигурации соединения '{connectionOptions.Name}' не заполнены обязательные свойства публикации события. " +
                    $"Свойства, требующие заполнения: '{optionsEmptyProperties.GetNames()}'");
            }
            
            // Проверка на дублирование
            var eventKey = EventKey.New(options.Key.TypeName);
            
            var publishKey = string.IsNullOrWhiteSpace(options.Key.RoutingKey)
                ? PublishKey.New(options.Key.Exchange, eventKey)
                : PublishKey.New(options.Key.Exchange, eventKey, RoutingKey.New(options.Key.RoutingKey));
            
            if (_publishOptionsMap.TryGetValue(publishKey, out var existsPublishOptions))
            {
                throw new InvalidConfigurationException(
                    $"Для события с ключом '{publishKey}' уже настроена публикация." +
                    $"{NewLine}Опции существующей публикации: '{Serialize(existsPublishOptions)}'.");
            }
            
            var contentType = string.IsNullOrWhiteSpace(options.Options.ContentType)
                ? MessageContentType.JSON
                : options.Options.ContentType;

            var behaviors = GetBehaviors(assemblies, options.Options.Behaviors).Distinct().ToList();

            var publishOptions = new PublishOptions(
                contentType,
                options.Options.RetryCount ?? 5,
                options.Options.DeliveryMode ?? 2,
                behaviors);
            
            _publishOptionsMap.Add(publishKey, publishOptions);
        }
    }

    /// <summary>
    /// Заполняет опции Rpc запросов
    /// </summary>
    /// <param name="connectionOptions">Опции соединения</param>
    /// <param name="assemblies">Список сборок</param>
    private void FillRpcRequestsOptions(
        ConnectionOptions connectionOptions,
        Assembly[] assemblies)
    {
        if (connectionOptions.RpcRequestsOptions.Count is 0)
        {
            MainLogger.Info($"Не указаны опции Rpc запросов для соединения '{connectionOptions.Name}'.");
            return;
        }

        foreach (var options in connectionOptions.RpcRequestsOptions)
        {
            if (!options.Key.CheckRequiredProperties(out var keyEmptyProperties))
            {
                throw new InvalidConfigurationException(
                    $"В конфигурации соединения '{connectionOptions.Name}' " +
                    $"не заполнены обязательные свойства Rpc запросов. " +
                    $"Свойства, требующие заполнения: '{keyEmptyProperties.GetNames()}'");
            }
            
            if (!options.Options.CheckRequiredProperties(out var optionsEmptyProperties))
            {
                throw new InvalidConfigurationException(
                    $"В конфигурации соединения '{connectionOptions.Name}' " +
                    $"не заполнены обязательные свойства Rpc запросов. " +
                    $"Свойства, требующие заполнения: '{optionsEmptyProperties.GetNames()}'");
            }
            
            // Проверка на дублирование
            var eventKey = EventKey.New(options.Key.TypeName);
            var routingKey = RoutingKey.New(options.Key.RoutingKey);
            var publishKey = PublishKey.New(options.Key.Exchange, eventKey, routingKey);
            
            if (_rpcRequestsOptionsMap.TryGetValue(publishKey, out var requestOptions))
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
            
            _rpcRequestsOptionsMap.Add(publishKey, rpcRequestOptions);
        }
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