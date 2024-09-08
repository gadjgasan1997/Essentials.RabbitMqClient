using Essentials.Utils.Extensions;
using Essentials.RabbitMqClient.Options;

namespace Essentials.RabbitMqClient.Configuration;

/// <summary>
/// Менеджер для получения конфигурации
/// </summary>
public abstract class ConfigurationManager
{
    internal static Dictionary<string, ModelOptions> ConnectionsModels { get; } = new();
    
    internal ModelOptions ModelOptions { get; }
    
    private protected ConfigurationManager(string connectionName)
    {
        connectionName.CheckNotNullOrEmpty("Название соединения RabbitMq не может быть пустым");
        
        if (ConnectionsModels.ContainsKey(connectionName))
            throw new ArgumentException($"Соединение с названием '{connectionName}' уже зарегистрировано");
        
        ModelOptions = new ModelOptions();
        ConnectionsModels.Add(connectionName, ModelOptions);
    }
}