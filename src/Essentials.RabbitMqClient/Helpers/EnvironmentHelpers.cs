using System.Net;

namespace Essentials.RabbitMqClient.Helpers;

/// <summary>
/// Хелперы для работы с окружением
/// </summary>
internal static class EnvironmentHelpers
{
    /// <summary>
    /// Возвращает название хоста
    /// </summary>
    /// <returns></returns>
    public static string GetHostName()
    {
        var hostName = Environment.GetEnvironmentVariable("HOST_NAME");
        return string.IsNullOrWhiteSpace(hostName) ? Dns.GetHostName() : hostName;
    }
}