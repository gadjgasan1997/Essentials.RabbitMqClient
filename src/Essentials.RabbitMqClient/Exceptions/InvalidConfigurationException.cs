namespace Essentials.RabbitMqClient.Exceptions;

/// <summary>
/// Исключение о неверной конфигураци
/// </summary>
public class InvalidConfigurationException : Exception
{
    internal InvalidConfigurationException(string message)
        : base(message)
    { }
    
    internal InvalidConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    { }
}