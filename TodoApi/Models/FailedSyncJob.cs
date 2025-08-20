namespace TodoApi.Models
{
    public class FailedSyncJob
    {
        public int Id { get; set; }
        public string Operation { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string Queue { get; set; } = "default"; // queue name/type
        public DateTime FailedAt { get; set; } = DateTime.UtcNow;
    }
}
