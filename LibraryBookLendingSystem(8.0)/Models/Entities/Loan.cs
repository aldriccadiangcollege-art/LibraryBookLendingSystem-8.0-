using System;

namespace LibraryBookLendingSystem.Models.Entities
{
    public class Loan
    {
        public int LoanId { get; set; }
        public int BorrowerId { get; set; }
        public Borrower Borrower { get; set; } = null!;
        public int CopyId { get; set; }
        public BookCopy BookCopy { get; set; } = null!;
        public int IssuedBy { get; set; }
        public User IssuedByUser { get; set; } = null!;
        public DateOnly LoanDate { get; set; }
        public DateOnly DueDate { get; set; }
        public DateOnly? ReturnDate { get; set; }
        public int? ReceivedBy { get; set; }
        public User? ReceivedByUser { get; set; }
        public decimal FineAmount { get; set; } = 0;
        public DateOnly? FinePaidOn { get; set; }
    }
}