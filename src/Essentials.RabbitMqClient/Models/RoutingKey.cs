using System.Text;
using Essentials.Utils.Extensions;
using Essentials.RabbitMqClient.Helpers;

namespace Essentials.RabbitMqClient.Models;

/// <summary>
/// Ключ маршрутизации
/// </summary>
public record RoutingKey
{
    private RoutingKey(string key)
    {
        Key = key.CheckNotNullOrEmpty().ReplaceEnvironmentVariables();
    }
    
    /// <summary>
    /// Ключ
    /// </summary>
    public string Key { get; }
    
    /// <summary>
    /// Создает ключ маршрутизации
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Ключ маршрутизации</returns>
    internal static RoutingKey New(string key) => new(key);
    
    /// <summary>
    /// Возвращает строковое представление ключа
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var builder = new StringBuilder();

        builder
            .Append("{ \"Ключ маршрутизации\": \"")
            .Append(Key)
            .Append("\" }");
        
        return builder.ToString();
    }
}