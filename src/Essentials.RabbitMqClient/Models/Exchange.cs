using Essentials.Utils.Extensions;

namespace Essentials.RabbitMqClient.Models;

/// <summary>
/// Обменник
/// </summary>
/// <param name="Name">Название</param>
/// <param name="Type">Тип</param>
/// <param name="Durable">Признак, что очередь сохраняет свое состояние</param>
/// <param name="AutoDelete">Признак, что очередь будет удалена автоматически</param>
internal record Exchange(
    string Name,
    string Type,
    bool Durable,
    bool AutoDelete)
{
    /// <summary>
    /// Название
    /// </summary>
    public string Name { get; } = Name.CheckNotNullOrEmpty();
    
    /// <summary>
    /// Тип
    /// </summary>
    public string Type { get; } = Type.CheckNotNullOrEmpty();

    /// <summary>
    /// Признак, что очередь сохраняет свое состояние
    /// </summary>
    public bool Durable { get; } = Durable;

    /// <summary>
    /// Признак, что очередь будет удалена автоматически
    /// </summary>
    public bool AutoDelete { get; } = AutoDelete;
}