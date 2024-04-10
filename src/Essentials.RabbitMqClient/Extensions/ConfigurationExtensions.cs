using Essentials.RabbitMqClient.Exceptions;
using Essentials.RabbitMqClient.Options;
using Microsoft.Extensions.Configuration;
using Essentials.Utils.Reflection.Extensions;

namespace Essentials.RabbitMqClient.Extensions;

/// <summary>
/// Методы расширения для <see cref="IConfiguration" />
/// </summary>
internal static class ConfigurationExtensions
{
    /// <summary>
    /// Возвращает опции соединений с RabbitMq из конфигурации
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static ConnectionsOptions GetConnectionsOptions(this IConfiguration configuration)
    {
        var section = configuration.GetSection(ConnectionsOptions.Section);
        if (!section.Exists())
            throw new InvalidConfigurationException("Не найдена секция с настройками соединений RabbitMq");
        
        var connectionsOptions = new ConnectionsOptions();
        section.Bind(connectionsOptions);
        
        foreach (var connectionOptions in connectionsOptions.Connections)
        {
            if (!connectionOptions.CheckRequiredProperties(out var emptyProperties))
            {
                throw new InvalidConfigurationException(
                    $"В конфигурации не заполнены обязательные свойства соединения: '{emptyProperties.GetNames()}'");
            }
        }

        return connectionsOptions;
    }
}