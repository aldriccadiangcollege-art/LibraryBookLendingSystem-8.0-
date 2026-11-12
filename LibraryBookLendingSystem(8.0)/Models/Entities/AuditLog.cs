using System;

namespace LibraryBookLendingSystem.Models.Entities
{
    public class AuditLog
    {
        public int AuditId { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }

        public string Action { get; set; } = null!;
        public string? TableName { get; set; }
        public string? RecordId { get; set; }
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}