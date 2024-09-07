using NLog;

namespace Essentials.RabbitMqClient.Dictionaries;

/// <summary>
/// Логгеры для взаиодействия с очередью
/// </summary>
public static class QueueLoggers
{
    /// <summary>
    /// Основной логгер
    /// </summary>
    public static Logger MainLogger { get; } = LogManager.GetLogger("Essentials.RabbtMqClient.Main");
}