using System.Text;
using Essentials.Utils.Extensions;
using Essentials.RabbitMqClient.Helpers;

namespace Essentials.RabbitMqClient.Models;

/// <summary>
/// Ключ очереди
/// </summary>
public record QueueKey
{
    private const string HOST_NAME = "{host_name}";
    
    private QueueKey(string queueName)
    {
        QueueName = queueName.CheckNotNullOrEmpty();

        if (QueueName.Contains(HOST_NAME))
            QueueName = QueueName.Replace(HOST_NAME, EnvironmentHelpers.GetHostName());
    }
    
    /// <summary>
    /// Название очереди
    /// </summary>
    public string QueueName { get; }

    /// <summary>
    /// Создает ключ очереди
    /// </summary>
    /// <param name="queueName">Название очереди</param>
    /// <returns>Ключ очереди</returns>
    internal static QueueKey New(string queueName) => new(queueName);

    /// <summary>
    /// Возвращает строковое представление ключа
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var builder = new StringBuilder();

        builder
            .Append("{ \"Название очереди\": \"")
            .Append(QueueName)
            .Append("\" }");
        
        return builder.ToString();
    }
}