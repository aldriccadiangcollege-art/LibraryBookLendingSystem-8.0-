using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryBookLendingSystem.Models.Entities;

[Table("Books")]
public class Book
{
    [Key] public int BookId { get; set; }

    [MaxLength(20)] public string? Isbn { get; set; }

    [Required, MaxLength(250)] public string Title { get; set; } = string.Empty;

    [Required, MaxLength(150)] public string Author { get; set; } = string.Empty;

    [MaxLength(150)] public string? Publisher { get; set; }

    public short? PublicationYear { get; set; }

    [Required, MaxLength(80)] public string Category { get; set; } = string.Empty;

    [MaxLength(50)] public string? ShelfLocation { get; set; }

    public bool IsDigitalResource { get; set; } = false;

    [MaxLength(500)] public string? AccessUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();
}