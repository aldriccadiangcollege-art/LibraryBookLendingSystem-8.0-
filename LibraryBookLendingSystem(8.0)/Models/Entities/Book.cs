using System;
using System.Collections.Generic;

namespace LibraryBookLendingSystem.Models.Entities
{
    public class Book
    {
        public int BookId { get; set; }
        public string? Isbn { get; set; }
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string? Publisher { get; set; }
        public short? PublicationYear { get; set; }
        public string Category { get; set; } = null!;
        public string? ShelfLocation { get; set; }
        public bool IsDigitalResource { get; set; } = false;
        public string? AccessUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<BookCopy> BookCopies { get; set; } = new List<BookCopy>();
    }
}