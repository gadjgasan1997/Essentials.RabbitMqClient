using MediatR;
using LanguageExt;

namespace Sample.Server.Queue.ScaleTest;

public class Handler : IRequestHandler<Input, Output>
{
    private static readonly Dictionary<string, int> _map = new()
    {
        ["Ivan"] = 12,
        ["Petr"] = 30
    };
    
    public Task<Output> Handle(Input request, CancellationToken cancellationToken)
    {
        return new Output
        {
            Age = _map.GetValueOrDefault(request.Name, -1)
        }.AsTask();
    }
}