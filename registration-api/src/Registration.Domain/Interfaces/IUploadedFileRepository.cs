namespace Registration.Domain.Interfaces
{
    public interface IUploadedFileRepository
    {
        Task<bool> ExistsByHashAsync(string hash);
        Task AddAsync(string hash);
    }
}