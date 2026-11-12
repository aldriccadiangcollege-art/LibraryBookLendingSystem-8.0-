using Microsoft.EntityFrameworkCore;
using LibraryBookLendingSystem.Models.Entities;

namespace LibraryBookLendingSystem.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

        public DbSet<SystemConfig> SystemConfigs => Set<SystemConfig>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Borrower> Borrowers => Set<Borrower>();
        public DbSet<Book> Books => Set<Book>();
        public DbSet<BookCopy> BookCopies => Set<BookCopy>();
        public DbSet<Loan> Loans => Set<Loan>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // SystemConfigs
            modelBuilder.Entity<SystemConfig>(e => {
                e.HasKey(x => x.ConfigKey);
                e.Property(x => x.ConfigKey).HasMaxLength(50);
                e.Property(x => x.ConfigValue).HasMaxLength(100).IsRequired();
            });

            // Roles
            modelBuilder.Entity<Role>(e => {
                e.HasKey(x => x.RoleId);
                e.HasIndex(x => x.RoleName).IsUnique();
            });

            // Users
            modelBuilder.Entity<User>(e => {
                e.HasKey(x => x.UserId);
                e.HasIndex(x => x.Username).IsUnique();
                e.HasIndex(x => x.Email).IsUnique();
            });

            // UserRoles (Composite Key)
            modelBuilder.Entity<UserRole>(e => {
                e.HasKey(ur => new { ur.UserId, ur.RoleId });
                e.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId);
                e.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId);
            });

            // Borrowers
            modelBuilder.Entity<Borrower>(e => {
                e.HasKey(x => x.BorrowerId);
                e.HasIndex(x => x.CardNo).IsUnique();
                e.HasIndex(x => x.Lrn).IsUnique().HasFilter("[Lrn] IS NOT NULL");
                e.HasIndex(x => x.UserId).IsUnique().HasFilter("[UserId] IS NOT NULL");
                e.HasOne(x => x.User).WithOne(u => u.Borrower).HasForeignKey<Borrower>(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
            });

            // Books
            modelBuilder.Entity<Book>(e => {
                e.HasKey(x => x.BookId);
                e.HasIndex(x => x.Isbn).IsUnique().HasFilter("[Isbn] IS NOT NULL");
                e.HasIndex(x => x.Title);
            });

            // BookCopies
            modelBuilder.Entity<BookCopy>(e => {
                e.HasKey(x => x.CopyId);
                e.HasIndex(x => x.AccessionNo).IsUnique();
                e.HasOne(x => x.Book).WithMany(b => b.BookCopies).HasForeignKey(x => x.BookId);
                e.HasOne(x => x.WithdrawnByUser).WithMany().HasForeignKey(x => x.WithdrawnBy).OnDelete(DeleteBehavior.SetNull);
            });

            // Loans
            modelBuilder.Entity<Loan>(e => {
                e.HasKey(x => x.LoanId);
                e.HasOne(x => x.Borrower).WithMany(b => b.Loans).HasForeignKey(x => x.BorrowerId);
                e.HasOne(x => x.BookCopy).WithMany(c => c.Loans).HasForeignKey(x => x.CopyId);
                e.HasOne(x => x.IssuedByUser).WithMany().HasForeignKey(x => x.IssuedBy).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.ReceivedByUser).WithMany().HasForeignKey(x => x.ReceivedBy).OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(x => x.CopyId).IsUnique().HasFilter("[ReturnDate] IS NULL"); // One active loan per copy
            });

            // AuditLogs
            modelBuilder.Entity<AuditLog>(e => {
                e.HasKey(x => x.AuditId);
                e.HasOne(x => x.User).WithMany(u => u.AuditLogs).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
