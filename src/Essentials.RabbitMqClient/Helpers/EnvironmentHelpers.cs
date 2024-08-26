using System.Net;
using System.Reflection;

namespace Essentials.RabbitMqClient.Helpers;

/// <summary>
/// Хелперы для работы с окружением
/// </summary>
internal static class EnvironmentHelpers
{
    private const string HOST_NAME = "{host_name}";
    private const string APPLICATION_NAME = "{app_name}";
    private const string APPLICATION_INSTANCE = "{app_instance}";
    
    /// <summary>
    /// Заменяет переменные в строки
    /// </summary>
    /// <param name="sourceString">Исходная строка</param>
    /// <returns>Строка с замененными переменными</returns>
    internal static string ReplaceEnvironmentVariables(this string sourceString)
    {
        if (sourceString.Contains(HOST_NAME))
            sourceString = sourceString.Replace(HOST_NAME, GetHostName());
        
        if (sourceString.Contains(APPLICATION_NAME) && GetApplicationName() is { } applicationName)
            sourceString = sourceString.Replace(APPLICATION_NAME, applicationName);

        if (sourceString.Contains(APPLICATION_INSTANCE))
            sourceString = sourceString.Replace(APPLICATION_INSTANCE, GetApplicationInstance());

        return sourceString;
    }
    
    /// <summary>
    /// Возвращает название хоста
    /// </summary>
    /// <returns>Название хоста</returns>
    private static string GetHostName()
    {
        var hostName = Environment.GetEnvironmentVariable("HOST_NAME");
        return string.IsNullOrWhiteSpace(hostName) ? Dns.GetHostName() : hostName;
    }

    /// <summary>
    /// Возвращает название приложения
    /// </summary>
    /// <returns>Название приложения</returns>
    private static string? GetApplicationName()
    {
        var applicationName = Environment.GetEnvironmentVariable("APPLICATION_NAME");
        return string.IsNullOrWhiteSpace(applicationName)
            ? Assembly.GetCallingAssembly().FullName
            : applicationName;
    }

    /// <summary>
    /// Возвращает инстанс приложения
    /// </summary>
    /// <returns>Инстанс приложения</returns>
    private static string GetApplicationInstance()
    {
        var applicationInstance = Environment.GetEnvironmentVariable("APPLICATION_INSTANCE");
        return string.IsNullOrWhiteSpace(applicationInstance) ? "instance" : applicationInstance;
    }
}