using Essentials.RabbitMqClient.Models;

namespace Essentials.RabbitMqClient.RabbitMqModelBuilder.Implementations;

/// <inheritdoc cref="IOptionsProvider" />
internal class OptionsProvider : IOptionsProvider
{
    /// <summary>
    /// Мапа ключей соединений на их опции
    /// </summary>
    private static readonly Dictionary<ConnectionKey, ConnectionOptions> _connectionsOptions = new();
    
    /// <inheritdoc cref="IOptionsProvider.AddConnectionOptions" />
    public void AddConnectionOptions(
        ConnectionKey connectionKey,
        IEnumerable<Queue> queuesForDeclare,
        IEnumerable<Exchange> exchangesForDeclare)
    {
        if (_connectionsOptions.ContainsKey(connectionKey))
            throw new InvalidOperationException($"Опции для соединения с ключом '{connectionKey}' уже существуют");

        var connectionOptions = new ConnectionOptions(
            queuesForDeclare: new List<Queue>(queuesForDeclare),
            exchangesForDeclare: new List<Exchange>(exchangesForDeclare));
        
        _connectionsOptions.Add(connectionKey, connectionOptions);
    }

    /// <inheritdoc cref="IOptionsProvider.GetQueuesForDeclare" />
    public IEnumerable<Queue> GetQueuesForDeclare(ConnectionKey connectionKey)
    {
        var connectionOptions = GetConnectionOptions(connectionKey);
        return connectionOptions.QueuesForDeclare;
    }

    /// <inheritdoc cref="IOptionsProvider.GetExchangesForDeclare" />
    public IEnumerable<Exchange> GetExchangesForDeclare(ConnectionKey connectionKey)
    {
        var connectionOptions = GetConnectionOptions(connectionKey);
        return connectionOptions.ExchangesForDeclare;
    }
    
    /// <summary>
    /// Возвращает опции соединения по ключу
    /// </summary>
    /// <param name="connectionKey">Ключ соединения</param>
    /// <returns>Опции соединения</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    private static ConnectionOptions GetConnectionOptions(ConnectionKey connectionKey)
    {
        if (!_connectionsOptions.TryGetValue(connectionKey, out var options))
            throw new KeyNotFoundException($"Не найдены опции соединения с ключом '{connectionKey}'");

        return options;
    }

    /// <summary>
    /// Опции соединения
    /// </summary>
    private class ConnectionOptions
    {
        public ConnectionOptions(
            IEnumerable<Queue> queuesForDeclare,
            IEnumerable<Exchange> exchangesForDeclare)
        {
            QueuesForDeclare = queuesForDeclare;
            ExchangesForDeclare = exchangesForDeclare;
        }
        
        /// <summary>
        /// Список очередей для объявления
        /// </summary>
        public IEnumerable<Queue> QueuesForDeclare { get; }
        
        /// <summary>
        /// Список обменников для объявления
        /// </summary>
        public IEnumerable<Exchange> ExchangesForDeclare { get; }
    }
}