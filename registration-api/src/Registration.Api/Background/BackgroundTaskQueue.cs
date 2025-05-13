namespace Registration.Api.Background
{
    using System.Threading.Channels;
    using Registration.Domain.Interfaces;

    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<Func<CancellationToken, IRegistrationService, Task>> _queue;

        public BackgroundTaskQueue(int capacity = 100)
        {
            _queue = Channel.CreateBounded<Func<CancellationToken, IRegistrationService, Task>>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            });
        }

        public async ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, Task> workItem)
        {
            if (workItem == null) throw new ArgumentNullException(nameof(workItem));
            await _queue.Writer.WriteAsync((ct, _) => workItem(ct));
        }

        public async Task<Func<CancellationToken, IRegistrationService, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            var workItem = await _queue.Reader.ReadAsync(cancellationToken);
            return workItem;
        }
    }
}