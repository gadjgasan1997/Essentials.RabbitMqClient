using Essentials.RabbitMqClient;
using Essentials.RabbitMqClient.Publisher;

namespace Sample.Server.Queue.ScaleTest;

public class Handler : IEventHandler<Input>
{
    private static readonly Dictionary<string, int> _map = new()
    {
        ["Ivan"] = 12,
        ["Petr"] = 30
    };
    
    private readonly IEventsPublisher _eventsPublisher;
    
    public Handler(IEventsPublisher eventsPublisher)
    {
        _eventsPublisher = eventsPublisher;
    }
    
    public Task HandleAsync(Input @event)
    {
        _eventsPublisher.PublishRpcResponse(
            new Output
            {
                Age = _map[@event.Name]
            });

        return Task.CompletedTask;
    }
}