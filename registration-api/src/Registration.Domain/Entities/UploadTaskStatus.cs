namespace Registration.Domain.Entities
{
    public class UploadTaskStatus
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FileHash { get; set; } = null!;
        public string Status { get; set; } = "Pending"; // Pending / Processing / Completed / Failed
        public string? ErrorMessage { get; set; }

        public int SubmittedRecords { get; set; }
        public int ProcessedRecords { get; set; }
        public int InvalidRecords { get; set; }
        public int AddedRecords { get; set; }
        public int UpdatedRecords { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}