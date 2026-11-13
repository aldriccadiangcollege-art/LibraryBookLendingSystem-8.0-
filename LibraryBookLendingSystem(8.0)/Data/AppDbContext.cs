using LibraryBookLendingSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryBookLendingSystem.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SystemConfig> SystemConfigs => Set<SystemConfig>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Borrower> Borrowers => Set<Borrower>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookCopy> BookCopies => Set<BookCopy>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ---- SystemConfigs ----
        mb.Entity<SystemConfig>(e =>
        {
            e.HasKey(x => x.ConfigKey);
            e.Property(x => x.ConfigKey).HasMaxLength(50);
            e.Property(x => x.ConfigValue).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(255);
        });

        // ---- Roles ----
        mb.Entity<Role>(e =>
        {
            e.HasKey(x => x.RoleId);
            e.Property(x => x.RoleName).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.RoleName).IsUnique();
        });

        // ---- Users ----
        mb.Entity<User>(e =>
        {
            e.HasKey(x => x.UserId);
            e.Property(x => x.Username).HasMaxLength(50).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            e.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            e.Property(x => x.Email).HasMaxLength(120).IsRequired();
            e.HasIndex(x => x.Username).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.FailedLoginCount).HasDefaultValue(0);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
            e.ToTable(t => t.HasCheckConstraint("CK_Users_Failed", "[FailedLoginCount] >= 0"));
        });

        // ---- UserRoles (composite key, no non-key columns = 2NF) ----
        mb.Entity<UserRole>(e =>
        {
            e.HasKey(x => new { x.UserId, x.RoleId });
            e.HasOne(x => x.User).WithMany(u => u.UserRoles)
             .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Role).WithMany(r => r.UserRoles)
             .HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        });

        // ---- Borrowers ----
        mb.Entity<Borrower>(e =>
        {
            e.HasKey(x => x.BorrowerId);
            e.Property(x => x.CardNo).HasMaxLength(30).IsRequired();
            e.Property(x => x.Lrn).HasMaxLength(12);
            e.Property(x => x.FirstName).HasMaxLength(60).IsRequired();
            e.Property(x => x.MiddleName).HasMaxLength(60);
            e.Property(x => x.LastName).HasMaxLength(60).IsRequired();
            e.Property(x => x.BorrowerType).HasMaxLength(20).HasDefaultValue("Student");
            e.Property(x => x.Section).HasMaxLength(50);
            e.Property(x => x.ContactNo).HasMaxLength(20);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
            e.HasIndex(x => x.CardNo).IsUnique();
            e.HasIndex(x => x.Lrn).IsUnique().HasFilter("[Lrn] IS NOT NULL");
            e.HasIndex(x => x.UserId).IsUnique().HasFilter("[UserId] IS NOT NULL");
            e.HasIndex(x => new { x.LastName, x.FirstName });
            e.HasOne(x => x.User).WithMany()
             .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Borrowers_Type",
                    "[BorrowerType] IN ('Student','Teacher','Staff')");
                t.HasCheckConstraint("CK_Borrowers_Grade",
                    "[GradeLevel] BETWEEN 7 AND 12");
            });
        });

        // ---- Books ----
        mb.Entity<Book>(e =>
        {
            e.HasKey(x => x.BookId);
            e.Property(x => x.Isbn).HasMaxLength(20);
            e.Property(x => x.Title).HasMaxLength(250).IsRequired();
            e.Property(x => x.Author).HasMaxLength(150).IsRequired();
            e.Property(x => x.Publisher).HasMaxLength(150);
            e.Property(x => x.Category).HasMaxLength(80).IsRequired();
            e.Property(x => x.ShelfLocation).HasMaxLength(50);
            e.Property(x => x.AccessUrl).HasMaxLength(500);
            e.Property(x => x.IsDigitalResource).HasDefaultValue(false);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
            e.HasIndex(x => x.Isbn).IsUnique().HasFilter("[Isbn] IS NOT NULL");
            e.HasIndex(x => x.Title);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Books_Year",
                    "[PublicationYear] BETWEEN 1400 AND 2100");
                t.HasCheckConstraint("CK_Books_Digital",
                    "[IsDigitalResource] = 0 OR [AccessUrl] IS NOT NULL");
            });
        });

        // ---- BookCopies ----
        mb.Entity<BookCopy>(e =>
        {
            e.HasKey(x => x.CopyId);
            e.Property(x => x.AccessionNo).HasMaxLength(30).IsRequired();
            e.Property(x => x.CopyStatus).HasMaxLength(20).HasDefaultValue("Available");
            e.Property(x => x.ReviewRemarks).HasMaxLength(255);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
            e.HasIndex(x => x.AccessionNo).IsUnique();
            e.HasIndex(x => x.BookId);
            e.HasOne(x => x.Book).WithMany(b => b.Copies)
             .HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.WithdrawnByUser).WithMany()
             .HasForeignKey(x => x.WithdrawnBy).OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_BookCopies_Status",
                    "[CopyStatus] IN ('Available','Damaged','Lost','Withdrawn')");
                t.HasCheckConstraint("CK_BookCopies_Withdrawn",
                    "[CopyStatus] = 'Withdrawn' OR ([WithdrawnOn] IS NULL AND [WithdrawnBy] IS NULL)");
            });
        });

        // ---- Loans ----
        mb.Entity<Loan>(e =>
        {
            e.HasKey(x => x.LoanId);
            e.Property(x => x.FineAmount).HasColumnType("decimal(10,2)").HasDefaultValue(0m);
            e.Property(x => x.LoanDate).HasDefaultValueSql("CONVERT(DATE, SYSDATETIME())");
            e.HasIndex(x => x.BorrowerId);
            e.HasIndex(x => x.CopyId);
            e.HasIndex(x => x.CopyId).IsUnique()
             .HasFilter("[ReturnDate] IS NULL")
             .HasDatabaseName("UX_Loans_ActiveCopy");
            e.HasOne(x => x.Borrower).WithMany()
             .HasForeignKey(x => x.BorrowerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Copy).WithMany()
             .HasForeignKey(x => x.CopyId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.IssuedByUser).WithMany()
             .HasForeignKey(x => x.IssuedBy).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ReceivedByUser).WithMany()
             .HasForeignKey(x => x.ReceivedBy).OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Loans_Fine", "[FineAmount] >= 0");
                t.HasCheckConstraint("CK_Loans_Dates",
                    "[DueDate] >= [LoanDate] AND ([ReturnDate] IS NULL OR [ReturnDate] >= [LoanDate])");
            });
        });

        // ---- AuditLogs ----
        mb.Entity<AuditLog>(e =>
        {
            e.HasKey(x => x.AuditId);
            e.Property(x => x.Action).HasMaxLength(30).IsRequired();
            e.Property(x => x.TableName).HasMaxLength(50);
            e.Property(x => x.RecordId).HasMaxLength(50);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
            e.HasIndex(x => x.CreatedAt);
            e.HasIndex(x => x.UserId);
            e.HasOne(x => x.User).WithMany()
             .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
            e.ToTable(t => t.HasCheckConstraint("CK_AuditLogs_Action",
                "[Action] IN ('INSERT','UPDATE','DELETE','LOGIN','LOGIN_FAILED','LOGOUT'," +
                "'ISSUE','RETURN','FINE_PAID','IMPORT','EXPORT')"));
        });
    }
}