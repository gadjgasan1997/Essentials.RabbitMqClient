// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Essentials.RabbitMqClient.Options;

/// <summary>
/// Опции Ssl соединений
/// </summary>
internal class SslOptions
{
    /// <summary>
    /// Признак необходимости использовать Ssl соединение
    /// </summary>
    public bool Enable { get; set; }
    
    /// <summary>
    /// Путь к сертификату
    /// </summary>
    public string? CertPath { get; set; }
    
    /// <summary>
    /// Пароль для сертификата
    /// </summary>
    public string? CertPassphrase { get; set; }
    
    /// <summary>
    /// Путь к файлу с ключом
    /// </summary>
    public string? KeyFilePath { get; set; }

    /// <summary>
    /// Название Ssl сервера
    /// </summary>
    public string? SslServerName { get; set; }
}