using LanguageExt;
using Essentials.RabbitMqClient.Exceptions;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Essentials.Utils.Extensions;
using static LanguageExt.Prelude;

namespace Essentials.RabbitMqClient.Publisher.Implementations;

/// <inheritdoc cref="IAskManager" />
[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
internal class AskManager : IAskManager
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource<IEvent>> _tasks = new();

    /// <inheritdoc cref="IAskManager.GetCreateAsk(string)" />
    public Try<TaskCompletionSource<IEvent>> GetCreateAsk(string key) => Try(() =>
    {
        key.CheckNotNullOrEmpty("Не указан ключ для задачи, чтобы поставить в очередь");
        return _tasks.AddOrUpdate(
            key,
            new TaskCompletionSource<IEvent>(TaskCreationOptions.None),
            (_, val) => val);
    });

    /// <inheritdoc cref="IAskManager.SetAnswer(string, IEvent)" />
    public Try<Unit> SetAnswer(string key, IEvent result) => Try(() =>
    {
        key.CheckNotNullOrEmpty("Не указан ключ для задачи, чтобы установить ответ");
        if (_tasks.TryRemove(key, out var existingTask) && !existingTask.TrySetResult(result))
            throw new InvalidAskAttemptException($"Не удалось получить ответ и установить результат для задачи id: {key}");
        
        return Unit.Default;
    });

    /// <inheritdoc cref="IAskManager.Cancel(string, Exception)" />
    public Try<Unit> Cancel(string key, Exception ex) => Try(() =>
    {
        key.CheckNotNullOrEmpty("Не указан ключ для задачи на удаление");
        if (!_tasks.TryRemove(key, out var existingTask))
            throw new InvalidAskAttemptException($"Не удалось удалить задачу на выполнение id: {key}");
        
        if (!existingTask.TrySetException(ex))
            existingTask.SetCanceled();

        return Unit.Default;
    });
}