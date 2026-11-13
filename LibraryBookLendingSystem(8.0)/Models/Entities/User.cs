using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryBookLendingSystem.Models.Entities;

[Table("Users")]
public class User
{
    [Key] public int UserId { get; set; }
    [Required, MaxLength(50)] public string Username { get; set; } = string.Empty;
    [Required, MaxLength(255)] public string PasswordHash { get; set; } = string.Empty;
    [Required, MaxLength(120)] public string FullName { get; set; } = string.Empty;
    [Required, MaxLength(120)] public string Email { get; set; } = string.Empty;
    public int FailedLoginCount { get; set; } = 0;
    public DateTime? LockoutUntil { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}