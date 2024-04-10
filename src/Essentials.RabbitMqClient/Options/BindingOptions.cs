using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Essentials.RabbitMqClient.Options;

/// <summary>
/// Опции привязки очереди
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal class BindingOptions
{
    /// <summary>
    /// Обменник
    /// </summary>
    [Required]
    public string Exchange { get; set; } = null!;

    /// <summary>
    /// Ключ маршрутизации
    /// </summary>
    [Required]
    public string RoutingKey { get; set; } = null!;
}