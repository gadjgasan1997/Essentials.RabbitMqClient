using System.Text;
using Essentials.Utils.Extensions;
// ReSharper disable MemberCanBePrivate.Global

namespace Essentials.RabbitMqClient.Models;

/// <summary>
/// Ключ соединения
/// </summary>
public record ConnectionKey
{
    private ConnectionKey(string? connectionName)
    {
        connectionName.CheckNotNullOrEmpty(
            $"Название соединения для ключа не может быть пустым: '{connectionName}'");
        
        ConnectionName = connectionName.FullTrim()!.ToLowerInvariant();
    }
    
    /// <summary>
    /// Название соединения
    /// </summary>
    public string ConnectionName { get; }

    /// <summary>
    /// Создает ключ по названию соединения
    /// </summary>
    /// <param name="connectionName">Название соединения</param>
    /// <returns>Ключ</returns>
    internal static ConnectionKey New(string connectionName) => new(connectionName);

    /// <summary>
    /// Возвращает строковое представление ключа
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var builder = new StringBuilder();

        builder
            .Append("{ \"Название соединения\": \"")
            .Append(ConnectionName)
            .Append("\" }");
        
        return builder.ToString();
    }
}