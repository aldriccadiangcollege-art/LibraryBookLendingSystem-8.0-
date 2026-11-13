using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryBookLendingSystem.Models.Entities;

[Table("UserRoles")]
public class UserRole
{
    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))] public User? User { get; set; }
    public int RoleId { get; set; }
    [ForeignKey(nameof(RoleId))] public Role? Role { get; set; }
}