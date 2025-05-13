namespace Registration.Api.Background
{
    using Registration.Domain.Interfaces;

    public interface IBackgroundTaskQueue
    {
        ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, Task> workItem);
        Task<Func<CancellationToken, IRegistrationService, Task>> DequeueAsync(CancellationToken cancellationToken);
    }
}