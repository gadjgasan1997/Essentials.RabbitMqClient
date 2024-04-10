using System.ComponentModel.DataAnnotations;

namespace Essentials.RabbitMqClient.Options;

/// <summary>
/// Информация о перехватчике
/// </summary>
internal class BehaviorInfo
{
    /// <summary>
    /// Название типа перехватчика
    /// </summary>
    [Required]
    public string Name { get; set; } = null!;
}