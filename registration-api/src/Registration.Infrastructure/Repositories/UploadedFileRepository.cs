namespace Registration.Infrastructure.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using Registration.Domain.Interfaces;
    using Registration.Infrastructure.Persistence;
    using Registration.Domain.Entities;

    public class UploadedFileRepository : IUploadedFileRepository
    {
        private readonly ApplicationDbContext _context;

        public UploadedFileRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsByHashAsync(string hash)
        {
            return await _context.UploadedFiles.AnyAsync(f => f.Hash == hash);
        }

        public async Task AddAsync(string hash)
        {
            await _context.UploadedFiles.AddAsync(new UploadedFile
            {
                Hash = hash,
                UploadedAt = DateTime.UtcNow
            });
        }
    }
}