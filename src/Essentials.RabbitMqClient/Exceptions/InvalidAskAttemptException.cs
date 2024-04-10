namespace Essentials.RabbitMqClient.Exceptions;

/// <summary>
/// Исключение о неудачной попытке получить ответ после публикации
/// </summary>
public class InvalidAskAttemptException : Exception
{
    internal InvalidAskAttemptException(string message)
        : base(message)
    { }
}