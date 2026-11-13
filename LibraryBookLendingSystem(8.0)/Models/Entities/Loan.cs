using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryBookLendingSystem.Models.Entities;

[Table("Loans")]
public class Loan
{
    [Key] public int LoanId { get; set; }

    public int BorrowerId { get; set; }
    [ForeignKey(nameof(BorrowerId))] public Borrower? Borrower { get; set; }

    public int CopyId { get; set; }
    [ForeignKey(nameof(CopyId))] public BookCopy? Copy { get; set; }

    public int IssuedBy { get; set; }
    [ForeignKey(nameof(IssuedBy))] public User? IssuedByUser { get; set; }

    public DateTime LoanDate { get; set; } = DateTime.Today;
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }

    public int? ReceivedBy { get; set; }
    [ForeignKey(nameof(ReceivedBy))] public User? ReceivedByUser { get; set; }

    [Column(TypeName = "decimal(10,2)")] public decimal FineAmount { get; set; } = 0m;
    public DateTime? FinePaidOn { get; set; }
}