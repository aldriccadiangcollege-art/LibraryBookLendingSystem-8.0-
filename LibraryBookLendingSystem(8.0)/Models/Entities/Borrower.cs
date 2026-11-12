using System;
using System.Collections.Generic;

namespace LibraryBookLendingSystem.Models.Entities
{
    public class Borrower
    {
        public int BorrowerId { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }

        public string CardNo { get; set; } = null!;
        public string? Lrn { get; set; }
        public string FirstName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = null!;
        public string BorrowerType { get; set; } = "Student";
        public byte? GradeLevel { get; set; }
        public string? Section { get; set; }
        public string? ContactNo { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}