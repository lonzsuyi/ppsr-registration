namespace Registration.Api.Background
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Registration.Domain.Interfaces;

    public class CsvProcessingWorker : BackgroundService
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CsvProcessingWorker> _logger;

        public CsvProcessingWorker(
            IBackgroundTaskQueue taskQueue,
            IServiceProvider serviceProvider,
            ILogger<CsvProcessingWorker> logger)
        {
            _taskQueue = taskQueue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CSV background worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var scopedService = scope.ServiceProvider.GetRequiredService<IRegistrationService>();

                    await workItem(stoppingToken, scopedService);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing background CSV task.");
                }
            }

            _logger.LogInformation("CSV background worker stopping.");
        }
    }
}