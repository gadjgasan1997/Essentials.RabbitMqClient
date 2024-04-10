using System.Reflection;

namespace Essentials.RabbitMqClient.Extensions;

/// <summary>
/// Методы расширения для <see cref="PropertyInfo" />
/// </summary>
internal static class PropertyInfoExtensions
{
    /// <summary>
    /// Возврашает названия списка свойств через разделитель
    /// </summary>
    /// <param name="properties">Свойства</param>
    /// <returns></returns>
    public static string GetNames(this IEnumerable<PropertyInfo> properties) =>
        string.Join("', '", properties.Select(info => info.Name));
}