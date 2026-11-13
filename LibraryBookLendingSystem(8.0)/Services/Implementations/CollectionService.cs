using Microsoft.EntityFrameworkCore;
using LibraryBookLendingSystem.Data;
using LibraryBookLendingSystem_8._0_.Models.DTOs.Books;
using LibraryBookLendingSystem_8._0_.Services.Interfaces;

namespace LibraryBookLendingSystem_8._0_.Services.Implementations
{
    public class CollectionService : ICollectionService
    {
        private static readonly string[] Filters = { "outdated", "duplicates", "damaged", "lost", "withdrawn" };
        private const int OutdatedAfterYears = 10;

        private readonly LibraryDbContext _db;
        public CollectionService(LibraryDbContext db) => _db = db;

        public async Task<ServiceResult<CollectionReviewDto>> GetReviewAsync(string? filter)
        {
            filter = string.IsNullOrWhiteSpace(filter) ? null : filter.Trim().ToLowerInvariant();
            if (filter != null && !Filters.Contains(filter))
                return ServiceResult<CollectionReviewDto>.Fail(400, $"Filter must be one of: {string.Join(", ", Filters)}.");

            // Counts are computed in SQL through EF (no stored view needed); the small per-title list is flagged in memory.
            var items = await _db.Books.AsNoTracking()
                .OrderBy(b => b.Title)
                .Select(b => new CollectionReviewItemDto
                {
                    BookId = b.BookId,
                    Isbn = b.Isbn,
                    Title = b.Title,
                    Author = b.Author,
                    Category = b.Category,
                    PublicationYear = b.PublicationYear,
                    IsDigitalResource = b.IsDigitalResource,
                    TotalCopies = b.BookCopies.Count(),
                    OnLoan = b.BookCopies.Count(c => c.Loans.Any(l => l.ReturnDate == null)),
                    AvailableNow = b.BookCopies.Count(c => c.CopyStatus == "Available" && !c.Loans.Any(l => l.ReturnDate == null)),
                    DamagedCopies = b.BookCopies.Count(c => c.CopyStatus == "Damaged"),
                    LostCopies = b.BookCopies.Count(c => c.CopyStatus == "Lost"),
                    WithdrawnCopies = b.BookCopies.Count(c => c.CopyStatus == "Withdrawn")
                })
                .ToListAsync();

            var cutoffYear = DateTime.Now.Year - OutdatedAfterYears;
            var groups = items.GroupBy(i => (i.Title.Trim().ToLowerInvariant(), i.Author.Trim().ToLowerInvariant()))
                              .Where(g => g.Count() > 1)
                              .ToList();
            var duplicateIds = groups.SelectMany(g => g.Select(i => i.BookId)).ToHashSet();

            foreach (var i in items)
            {
                i.IsOutdated = i.PublicationYear != null && i.PublicationYear <= cutoffYear;
                i.IsDuplicateTitle = duplicateIds.Contains(i.BookId);
            }

            var summary = new CollectionReviewSummaryDto
            {
                TotalTitles = items.Count,
                TotalCopies = items.Sum(i => i.TotalCopies),
                OutdatedTitles = items.Count(i => i.IsOutdated),
                DuplicateTitles = duplicateIds.Count,
                DuplicateGroups = groups.Count,
                DamagedCopies = items.Sum(i => i.DamagedCopies),
                LostCopies = items.Sum(i => i.LostCopies),
                WithdrawnCopies = items.Sum(i => i.WithdrawnCopies)
            };

            IEnumerable<CollectionReviewItemDto> shown = filter switch
            {
                "outdated" => items.Where(i => i.IsOutdated),
                "duplicates" => items.Where(i => i.IsDuplicateTitle),
                "damaged" => items.Where(i => i.DamagedCopies > 0),
                "lost" => items.Where(i => i.LostCopies > 0),
                "withdrawn" => items.Where(i => i.WithdrawnCopies > 0),
                _ => items
            };

            return ServiceResult<CollectionReviewDto>.Ok(new CollectionReviewDto { Summary = summary, Items = shown.ToList() });
        }
    }
}
