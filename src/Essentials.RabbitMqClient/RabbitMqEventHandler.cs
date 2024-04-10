using MediatR;
using Essentials.Utils.Extensions;

namespace Essentials.RabbitMqClient;

/// <summary>
/// Обработчик события от RabbitMqClient
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public class RabbitMqEventHandler<TEvent> : IEventHandler<TEvent>
    where TEvent : IEvent
{
    private readonly IMediator _mediator;
    
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="mediator">Медиатр</param>
    public RabbitMqEventHandler(IMediator mediator)
    {
        _mediator = mediator.CheckNotNull();
    }

    /// <inheritdoc cref="IEventHandler{TEvent}.HandleAsync" />
    public async Task HandleAsync(TEvent request) => await _mediator.Send(request);
}