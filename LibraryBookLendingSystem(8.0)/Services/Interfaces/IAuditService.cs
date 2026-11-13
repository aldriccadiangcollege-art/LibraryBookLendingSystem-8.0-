namespace LibraryBookLendingSystem_8._0_.Models.DTOs.Books;

public interface IAuditService
{
    /// <summary>Writes one AuditLogs row (action must be one of the values allowed by the SQL CHECK).</summary>
    Task LogAsync(int? userId, string action, string tableName, string? recordId, string? details = null);
}
