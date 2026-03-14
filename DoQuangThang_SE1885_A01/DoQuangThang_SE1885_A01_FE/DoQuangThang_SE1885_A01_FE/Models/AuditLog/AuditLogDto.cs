namespace DoQuangThang_SE1885_A01_FE.Models.AuditLog
{
    public class AuditLogDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Action { get; set; }
        public string TableName { get; set; }
        public DateTime Timestamp { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
    }
}
