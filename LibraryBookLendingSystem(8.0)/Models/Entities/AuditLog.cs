using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryBookLendingSystem.Models.Entities;

[Table("AuditLogs")]
public class AuditLog
{
    [Key] public int AuditId { get; set; }
    public int? UserId { get; set; }
    [ForeignKey(nameof(UserId))] public User? User { get; set; }
    [Required, MaxLength(30)] public string Action { get; set; } = string.Empty;
    [MaxLength(50)] public string? TableName { get; set; }
    [MaxLength(50)] public string? RecordId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}