using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable CollectionNeverUpdated.Global

namespace Essentials.RabbitMqClient.Options;

/// <summary>
/// Опции соединения с RabbitMq
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal class ConnectionOptions
{
    /// <summary>
    /// Название подключения
    /// </summary>
    [Required]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Хост очереди
    /// </summary>
    [Required]
    public string Host { get; set; } = null!;
        
    /// <summary>
    /// Порт очереди
    /// </summary>
    public int Port { get; set; }
        
    /// <summary>
    /// Виртуал хост очереди
    /// </summary>
    [Required]
    public string VirtualHost { get; set; } = null!;
        
    /// <summary>
    /// Логин для подключения
    /// </summary>
    [Required]
    public string UserName { get; set; } = null!;

    /// <summary>
    /// Пароль для подключения
    /// </summary>
    [Required]
    public string Password { get; set; } = null!;
        
    /// <summary>
    /// Количество попыток на подключение к очереди
    /// </summary>
    public int? ConnectRetryCount { get; set; }

    /// <summary>
    /// Опции Ssl соединения
    /// </summary>
    public SslOptions Ssl { get; set; } = new();
    
    /// <summary>
    /// 
    /// </summary>
    public bool? DispatchConsumersAsync { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public TimeSpan? RequestedHeartbeat { get; set; }
    
    /// <summary>
    /// Опции моделей
    /// </summary>
    public ModelOptions? Model { get; set; }
}