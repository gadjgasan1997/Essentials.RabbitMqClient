using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using Essentials.Utils.Reflection.Extensions;
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Essentials.RabbitMqClient.Options;

/// <summary>
/// Опции очереди
/// </summary>
internal class QueueOptions
{
    /// <summary>
    /// Название
    /// </summary>
    [Required]
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Признак, что очередь сохраняет свое состояние
    /// </summary>
    public bool? Durable { get; set; }
    
    /// <summary>
    /// Признак, что очередь разрешает подключаться только одному потребителю
    /// </summary>
    public bool? Exclusive { get; set; }
    
    /// <summary>
    /// Признак, что очередь будет удалена автоматически
    /// </summary>
    public bool? AutoDelete { get; set; }

    /// <summary>
    /// Опции привязки очереди
    /// </summary>
    public List<BindingOptions> Bindings { get; set; } = new();
    
    /// <summary>
    /// Проверяет опции привязок на заполнение
    /// </summary>
    /// <param name="emptyProperties">Незаполненные свойства</param>
    /// <returns></returns>
    public bool CheckBindingsOptionsForEmpty([NotNullWhen(false)] out List<PropertyInfo>? emptyProperties)
    {
        foreach (var binding in Bindings)
        {
            if (!binding.CheckRequiredProperties(out emptyProperties))
                return false;
        }

        emptyProperties = null;
        return true;
    }
}