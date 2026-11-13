namespace LibraryBookLendingSystem_8._0_.Models.DTOs.Books
{
    /// <summary>Body for creating or updating a book (title-level data only).</summary>
    public class BookUpsertDto
    {
        public string? Isbn { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string? Publisher { get; set; }
        public short? PublicationYear { get; set; }
        public string Category { get; set; } = "";
        public string? ShelfLocation { get; set; }
        public bool IsDigitalResource { get; set; }
        public string? AccessUrl { get; set; }
    }

    public class BookDto : BookUpsertDto
    {
        public int BookId { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableNow { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>Catalog columns only, safe to show to students.</summary>
    public class CatalogBookDto
    {
        public int BookId { get; set; }
        public string? Isbn { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string Category { get; set; } = "";
        public short? PublicationYear { get; set; }
        public string? ShelfLocation { get; set; }
        public bool IsDigitalResource { get; set; }
        public string? AccessUrl { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableNow { get; set; }
    }

    public class RowErrorDto
    {
        public int Row { get; set; }          // spreadsheet row number (header = row 1)
        public string? Field { get; set; }
        public string Code { get; set; } = "validation";   // "validation" or "duplicate-isbn"
        public string Message { get; set; } = "";
    }

    public class ImportResultDto
    {
        public int TotalRows { get; set; }
        public int Imported { get; set; }
        public string Message { get; set; } = "";
        public List<RowErrorDto> Errors { get; set; } = new();
    }

    // ---- copies ----
    public class CopyCreateDto { public string AccessionNo { get; set; } = ""; }
    public class CopyUpdateDto { public string AccessionNo { get; set; } = ""; }
    public class CopyStatusDto
    {
        public string Status { get; set; } = "";
        public string? Remarks { get; set; }
    }

    public class CopyDto
    {
        public int CopyId { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = "";
        public string AccessionNo { get; set; } = "";
        public string CopyStatus { get; set; } = "";
        public string? ReviewRemarks { get; set; }
        public DateOnly? WithdrawnOn { get; set; }
        public int? WithdrawnBy { get; set; }
        public string? WithdrawnByName { get; set; }
        public bool OnLoan { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ---- collection review ----
    public class CollectionReviewItemDto
    {
        public int BookId { get; set; }
        public string? Isbn { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string Category { get; set; } = "";
        public short? PublicationYear { get; set; }
        public bool IsDigitalResource { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableNow { get; set; }
        public int OnLoan { get; set; }
        public int DamagedCopies { get; set; }
        public int LostCopies { get; set; }
        public int WithdrawnCopies { get; set; }
        public bool IsOutdated { get; set; }
        public bool IsDuplicateTitle { get; set; }
    }

    public class CollectionReviewSummaryDto
    {
        public int TotalTitles { get; set; }
        public int TotalCopies { get; set; }
        public int OutdatedTitles { get; set; }
        public int DuplicateTitles { get; set; }
        public int DuplicateGroups { get; set; }
        public int DamagedCopies { get; set; }
        public int LostCopies { get; set; }
        public int WithdrawnCopies { get; set; }
    }

    public class CollectionReviewDto
    {
        public CollectionReviewSummaryDto Summary { get; set; } = new();
        public List<CollectionReviewItemDto> Items { get; set; } = new();
    }
}
