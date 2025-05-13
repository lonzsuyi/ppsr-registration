namespace Registration.Infrastructure.Repositories
{
    using Registration.Domain.Entities;
    using Registration.Domain.Interfaces;
    using Registration.Infrastructure.Persistence;
    using Registration.Domain.Dtos;

    public class UploadTaskStatusRepository : IUploadTaskStatusRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UploadTaskStatusRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UploadTaskStatus?> GetByIdAsync(Guid id)
        {
            return await _dbContext.UploadTaskStatuses.FindAsync(id);
        }

        public async Task<UploadTaskStatus> CreateAsync(string fileHash)
        {
            var task = new UploadTaskStatus
            {
                FileHash = fileHash,
                Status = "Pending"
            };

            await _dbContext.UploadTaskStatuses.AddAsync(task);
            await _dbContext.SaveChangesAsync();
            return task;
        }

        public async Task UpdateSummaryAsync(Guid id, UploadSummaryDto summary)
        {
            var task = await _dbContext.UploadTaskStatuses.FindAsync(id);
            if (task == null) return;

            task.SubmittedRecords = summary.SubmittedRecords;
            task.ProcessedRecords = summary.ProcessedRecords;
            task.InvalidRecords = summary.InvalidRecords;
            task.AddedRecords = summary.AddedRecords;
            task.UpdatedRecords = summary.UpdatedRecords;
            task.Status = "Processing";

            await _dbContext.SaveChangesAsync();
        }

        public async Task MarkAsCompletedAsync(Guid id)
        {
            var task = await _dbContext.UploadTaskStatuses.FindAsync(id);
            if (task == null) return;

            task.Status = "Completed";
            task.CompletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task MarkAsFailedAsync(Guid id, string error)
        {
            var task = await _dbContext.UploadTaskStatuses.FindAsync(id);
            if (task == null) return;

            task.Status = "Failed";
            task.ErrorMessage = error;

            await _dbContext.SaveChangesAsync();
        }
    }
}