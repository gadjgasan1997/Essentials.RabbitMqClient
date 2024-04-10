using Essentials.RabbitMqClient;

namespace Sample.Client.Models;

public class Output : IEvent
{
    public string Name { get; set; } = null!;
}