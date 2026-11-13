using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryBookLendingSystem.Models.Entities;

[Table("BookCopies")]
public class BookCopy
{
    [Key] public int CopyId { get; set; }
    public int BookId { get; set; }
    [ForeignKey(nameof(BookId))] public Book? Book { get; set; }
    [Required, MaxLength(30)] public string AccessionNo { get; set; } = string.Empty;
    [Required, MaxLength(20)] public string CopyStatus { get; set; } = "Available";
    [MaxLength(255)] public string? ReviewRemarks { get; set; }
    public DateTime? WithdrawnOn { get; set; }
    public int? WithdrawnBy { get; set; }
    [ForeignKey(nameof(WithdrawnBy))] public User? WithdrawnByUser { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}