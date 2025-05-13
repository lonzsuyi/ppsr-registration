namespace Registration.Domain.Dtos
{
    public class UploadSummaryDto
    {
        public int SubmittedRecords { get; set; }
        public int InvalidRecords { get; set; }
        public int ProcessedRecords { get; set; }
        public int UpdatedRecords { get; set; }
        public int AddedRecords { get; set; }
    }
}