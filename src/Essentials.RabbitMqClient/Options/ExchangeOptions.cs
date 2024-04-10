// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

using System.ComponentModel.DataAnnotations;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Essentials.RabbitMqClient.Options;

/// <summary>
/// Опции обменника
/// </summary>
internal class ExchangeOptions
{
    /// <summary>
    /// Название
    /// </summary>
    [Required]
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Тип
    /// </summary>
    [Required]
    public string Type { get; set; } = null!;
    
    /// <summary>
    /// Признак, что очередь сохраняет свое состояние
    /// </summary>
    public bool? Durable { get; set; }
    
    /// <summary>
    /// Признак, что очередь будет удалена автоматически
    /// </summary>
    public bool? AutoDelete { get; set; }
}