using Essentials.RabbitMqClient.Models;
using Essentials.RabbitMqClient.Options;
using Essentials.RabbitMqClient.Exceptions;
using Essentials.RabbitMqClient.Extensions;
using Essentials.Utils.Reflection.Extensions;
using static Essentials.RabbitMqClient.Dictionaries.QueueLoggers;

namespace Essentials.RabbitMqClient.RabbitMqModelBuilder;

/// <summary>
/// Билдер для <see cref="IOptionsProvider" />
/// </summary>
internal static class OptionsProviderBuilder
{
    /// <summary>
    /// Билдит провайдер получения опций
    /// </summary>
    /// <param name="connectionsOptions"></param>
    /// <returns></returns>
    public static Implementations.OptionsProvider Build(
        IReadOnlyDictionary<string, ModelOptions> connectionsOptions)
    {
        var provider = new Implementations.OptionsProvider();

        foreach (var (connectionName, modelOptions) in connectionsOptions)
            AddConnectionOptionsToProvider(provider, connectionName, modelOptions);

        return provider;
    }
    
    /// <summary>
    /// Добавляет опции соединения в провайдер
    /// </summary>
    /// <param name="provider">Провайдер</param>
    /// <param name="connectionName"></param>
    /// <param name="modelOptions"></param>
    private static void AddConnectionOptionsToProvider(
        Implementations.OptionsProvider provider,
        string connectionName,
        ModelOptions modelOptions)
    {
        var connectionKey = ConnectionKey.New(connectionName);
        var queues = GetQueuesList(connectionName, modelOptions.Queues);
        var exchanges = GetExchangesList(connectionName, modelOptions.Exchanges);
        
        provider.AddConnectionOptions(connectionKey, queues, exchanges);
    }

    /// <summary>
    /// Возвращает список очередей
    /// </summary>
    /// <param name="connectionName"></param>
    /// <param name="queuesOptions"></param>
    private static List<Queue> GetQueuesList(string connectionName, List<QueueOptions> queuesOptions)
    {
        if (queuesOptions.Count is 0)
        {
            MainLogger.Info($"Не указаны опции очередей для соединения '{connectionName}'.");
            return [];
        }
        
        var queues = new List<Queue>(queuesOptions.Count);
        foreach (var options in queuesOptions)
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
            if (queues.Select(queue => queue.QueueKey).Contains(queueKey))
            {
                throw new InvalidConfigurationException(
                    $"Конфигурация для очереди с ключом '{queueKey}' задублирована.");
            }

            var queueBindings = (
                    from binding in options.Bindings
                    let routingKey = RoutingKey.New(binding.RoutingKey)
                    select new Binding(binding.Exchange, routingKey))
                .ToList();

            var queueForDeclare = new Queue(
                queueKey,
                options.Durable ?? true,
                options.Exclusive ?? false,
                options.AutoDelete ?? false,
                queueBindings);

            queues.Add(queueForDeclare);
        }

        return queues;
    }

    /// <summary>
    /// Возвращает список обменников
    /// </summary>
    /// <param name="connectionName"></param>
    /// <param name="exchangesOptions"></param>
    private static List<Exchange> GetExchangesList(string connectionName, List<ExchangeOptions> exchangesOptions)
    {
        if (exchangesOptions.Count is 0)
        {
            MainLogger.Info($"Не указаны опции обменников для соединения '{connectionName}'.");
            return [];
        }

        var exchanges = new List<Exchange>();
        foreach (var options in exchangesOptions)
        {
            if (!options.CheckRequiredProperties(out var exchangeEmptyProperties))
            {
                throw new InvalidConfigurationException(
                    "В конфигурации не заполнены обязательные свойства обменника. " +
                    $"Свойства, требующие заполнения: '{exchangeEmptyProperties.GetNames()}'");
            }
            
            if (exchanges.Any(exchange => exchange.Name == options.Name))
                continue;
            
            var exchangeForDeclare = new Exchange(
                options.Name,
                options.Type,
                options.Durable ?? true,
                options.AutoDelete ?? false);
            
            exchanges.Add(exchangeForDeclare);
        }

        return exchanges;
    }
}