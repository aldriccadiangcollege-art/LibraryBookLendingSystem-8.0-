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
            modelBuilder.Entity<SystemConfig>(entity =>
            {
                entity.HasKey(e => e.ConfigKey);
                entity.Property(e => e.ConfigKey).HasMaxLength(50);
                entity.Property(e => e.ConfigValue).HasMaxLength(100).IsRequired();
            });

            // Roles
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleId);
                entity.HasIndex(e => e.RoleName).IsUnique();
            });

            // Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.FailedLoginCount).HasDefaultValue(0);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
            });

            // UserRoles (Composite Key)
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });
                entity.HasOne(ur => ur.User)
                      .WithMany(u => u.UserRoles)
                      .HasForeignKey(ur => ur.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ur => ur.Role)
                      .WithMany(r => r.UserRoles)
                      .HasForeignKey(ur => ur.RoleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Borrowers
            modelBuilder.Entity<Borrower>(entity =>
            {
                entity.HasKey(e => e.BorrowerId);
                entity.HasIndex(e => e.CardNo).IsUnique();
                entity.HasIndex(e => e.Lrn).IsUnique().HasFilter("[Lrn] IS NOT NULL");
                entity.HasIndex(e => e.UserId).IsUnique().HasFilter("[UserId] IS NOT NULL");
                entity.HasOne(e => e.User)
                      .WithOne(u => u.Borrower)
                      .HasForeignKey<Borrower>(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // AuditLogs
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.AuditId);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.AuditLogs)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}