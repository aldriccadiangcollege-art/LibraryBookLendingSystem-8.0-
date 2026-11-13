using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryBookLendingSystem.Models.Entities;

[Table("SystemConfigs")]
public class SystemConfig
{
    [Key, MaxLength(50)] public string ConfigKey { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string ConfigValue { get; set; } = string.Empty;
    [MaxLength(255)] public string? Description { get; set; }
}