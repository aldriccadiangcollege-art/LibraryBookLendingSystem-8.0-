using LibraryBookLendingSystem_8._0_.Data;
using LibraryBookLendingSystem_8._0_.Models.Entities;
using LibraryBookLendingSystem_8._0_.Models.DTOs.Books;

namespace LibraryBookLendingSystem_8._0_.Services.Implementations
{
    public class AuditService : IAuditService
    {
        private readonly LibraryDbContext _db;
        public AuditService(LibraryDbContext db) => _db = db;

        public async Task LogAsync(int? userId, string action, string tableName, string? recordId, string? details = null)
        {
            _db.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Action = action,
                TableName = tableName,
                RecordId = recordId,
                Details = details
            });
            await _db.SaveChangesAsync();
        }
    }
}
