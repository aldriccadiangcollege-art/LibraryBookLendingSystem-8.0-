using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryBookLendingSystem.Models.Entities;

[Table("Borrowers")]
public class Borrower
{
    [Key] public int BorrowerId { get; set; }
    public int? UserId { get; set; }
    [ForeignKey(nameof(UserId))] public User? User { get; set; }
    [Required, MaxLength(30)] public string CardNo { get; set; } = string.Empty;
    [MaxLength(12)] public string? Lrn { get; set; }
    [Required, MaxLength(60)] public string FirstName { get; set; } = string.Empty;
    [MaxLength(60)] public string? MiddleName { get; set; }
    [Required, MaxLength(60)] public string LastName { get; set; } = string.Empty;
    [Required, MaxLength(20)] public string BorrowerType { get; set; } = "Student";
    public byte? GradeLevel { get; set; }
    [MaxLength(50)] public string? Section { get; set; }
    [MaxLength(20)] public string? ContactNo { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}