namespace Registration.Domain.Entities
{
    public class UploadedFile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Hash { get; set; } = null!;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}