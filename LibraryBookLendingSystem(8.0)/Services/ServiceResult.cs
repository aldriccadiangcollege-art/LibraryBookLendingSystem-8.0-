using Microsoft.EntityFrameworkCore;

namespace LibraryBookLendingSystem_8._0_.Services
{
    /// <summary>Outcome of a service call: a value or an HTTP-style error (400/404/409...).</summary>
    public class ServiceResult<T>
    {
        public bool Success { get; private init; }
        public int StatusCode { get; private init; }
        public string? Error { get; private init; }
        public T? Value { get; private init; }

        public static ServiceResult<T> Ok(T value, int statusCode = 200) =>
            new() { Success = true, StatusCode = statusCode, Value = value };

        public static ServiceResult<T> Fail(int statusCode, string error, T? value = default) =>
            new() { Success = false, StatusCode = statusCode, Error = error, Value = value };
    }

    public static class DbErrors
    {
        /// <summary>True when SQL Server rejected the save because of a unique index (2601/2627).</summary>
        public static bool IsUniqueViolation(DbUpdateException ex) =>
            ex.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 };
    }

    public static class CopyStatuses
    {
        public const string Available = "Available";
        public const string Damaged = "Damaged";
        public const string Lost = "Lost";
        public const string Withdrawn = "Withdrawn";
        public static readonly string[] All = { Available, Damaged, Lost, Withdrawn };

    }  
}
