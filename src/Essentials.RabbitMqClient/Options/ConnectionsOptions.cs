using System.Diagnostics.CodeAnalysis;

namespace Essentials.RabbitMqClient.Options;

/// <summary>
/// Опции соединений
/// </summary>
[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
internal class ConnectionsOptions
{
    /// <summary>
    /// Название секции в конфигурации
    /// </summary>
    public static string Section => "RabbitMqOptions";
    
    /// <summary>
    /// Существующие соединения
    /// </summary>
    public List<ConnectionOptions> Connections { get; set; } = new();
}