namespace Registration.Domain.Interfaces
{
    using Registration.Domain.Entities;
    using Registration.Domain.Dtos;

    public interface IUploadTaskStatusRepository
    {
        Task<UploadTaskStatus?> GetByIdAsync(Guid id);
        Task<UploadTaskStatus> CreateAsync(string fileHash);
        Task UpdateSummaryAsync(Guid id, UploadSummaryDto summary);
        Task MarkAsFailedAsync(Guid id, string error);
        Task MarkAsCompletedAsync(Guid id);
    }
}