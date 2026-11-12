using System;
using System.Collections.Generic;

namespace LibraryBookLendingSystem.Models.Entities
{
    public class BookCopy
    {
        public int CopyId { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; } = null!;
        public string AccessionNo { get; set; } = null!;
        public string CopyStatus { get; set; } = "Available"; // Available, Damaged, Lost, Withdrawn
        public string? ReviewRemarks { get; set; }
        public DateOnly? WithdrawnOn { get; set; }
        public int? WithdrawnBy { get; set; }
        public User? WithdrawnByUser { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}