using LanguageExt;

namespace Essentials.RabbitMqClient.Publisher;

/// <summary>
///     Вспомогательный менеджер для синхронизации ответов из очередей после публикации (RPC)
/// </summary>
internal interface IAskManager
{
    /// <summary>
    ///     Получить задачу на получение ответа из очереди
    /// </summary>
    Try<TaskCompletionSource<IEvent>> GetCreateAsk(string key);

    /// <summary>
    ///     Установить ответ
    /// </summary>
    Try<Unit> SetAnswer(string key, IEvent result);

    /// <summary>
    ///     Установить ошибку для задачи и отменить
    /// </summary>
    Try<Unit> Cancel(string key, Exception ex);
}