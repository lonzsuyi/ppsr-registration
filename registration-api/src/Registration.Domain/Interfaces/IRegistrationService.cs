namespace Registration.Domain.Interfaces
{
    using Registration.Domain.Dtos;

    public interface IRegistrationService
    {
        Task<UploadSummaryDto> ProcessUploadAsync(Stream csvStream);
        Task<UploadSummaryDto> ProcessUploadBigFileAsync(Stream csvStream);
    }
}