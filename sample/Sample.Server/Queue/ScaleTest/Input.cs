using MediatR;
using Essentials.RabbitMqClient;

namespace Sample.Server.Queue.ScaleTest;

public class Input : IEvent, IRequest<Output>
{
    public string Name { get; set; } = null!;
}