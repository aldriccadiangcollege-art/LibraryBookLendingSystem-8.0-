using Microsoft.EntityFrameworkCore;
using LibraryBookLendingSystem_8._0_.Data;
using LibraryBookLendingSystem_8._0_.Models.DTOs.Books;
using LibraryBookLendingSystem_8._0_.Services.Interfaces;
using LibraryBookLendingSystem_8._0_.Models.Entities;


namespace LibraryBookLendingSystem_8._0_.Services.Implementations
{
    public class BookCopyService : IBookCopyService
    {
        private readonly LibraryDbContext _db;
        private readonly IAuditService _audit;

        public BookCopyService(LibraryDbContext db, IAuditService audit)
        {
            _db = db;
            _audit = audit;
        }

        private static IQueryable<CopyDto> ToDto(IQueryable<BookCopy> q) =>
            q.Select(c => new CopyDto
            {
                CopyId = c.CopyId,
                BookId = c.BookId,
                BookTitle = c.Book.Title,
                AccessionNo = c.AccessionNo,
                CopyStatus = c.CopyStatus,
                ReviewRemarks = c.ReviewRemarks,
                WithdrawnOn = c.WithdrawnOn,
                WithdrawnBy = c.WithdrawnBy,
                WithdrawnByName = c.WithdrawnByUser != null ? c.WithdrawnByUser.FullName : null,
                OnLoan = c.Loans.Any(l => l.ReturnDate == null),
                CreatedAt = c.CreatedAt
            });

        public async Task<ServiceResult<List<CopyDto>>> GetByBookAsync(int bookId)
        {
            if (!await _db.Books.AnyAsync(b => b.BookId == bookId))
                return ServiceResult<List<CopyDto>>.Fail(404, $"Book {bookId} was not found.");

            var list = await ToDto(_db.BookCopies.AsNoTracking().Where(c => c.BookId == bookId).OrderBy(c => c.AccessionNo)).ToListAsync();
            return ServiceResult<List<CopyDto>>.Ok(list);
        }

        public async Task<ServiceResult<CopyDto>> GetAsync(int copyId)
        {
            var dto = await ToDto(_db.BookCopies.AsNoTracking().Where(c => c.CopyId == copyId)).FirstOrDefaultAsync();
            return dto == null
                ? ServiceResult<CopyDto>.Fail(404, $"Copy {copyId} was not found.")
                : ServiceResult<CopyDto>.Ok(dto);
        }

        public async Task<ServiceResult<CopyDto>> CreateAsync(int bookId, CopyCreateDto dto, int? userId)
        {
            var book = await _db.Books.AsNoTracking().FirstOrDefaultAsync(b => b.BookId == bookId);
            if (book == null) return ServiceResult<CopyDto>.Fail(404, $"Book {bookId} was not found.");
            if (book.IsDigitalResource)
                return ServiceResult<CopyDto>.Fail(400, "A digital resource has no physical copies. Open it through its AccessUrl.");

            var accession = (dto.AccessionNo ?? "").Trim();
            var problem = ValidateAccession(accession);
            if (problem != null) return ServiceResult<CopyDto>.Fail(400, problem);

            if (await _db.BookCopies.AnyAsync(c => c.AccessionNo == accession))
                return ServiceResult<CopyDto>.Fail(409, $"Accession number {accession} is already used by another copy.");

            var copy = new BookCopy { BookId = bookId, AccessionNo = accession, CopyStatus = CopyStatuses.Available };
            _db.BookCopies.Add(copy);
            try { await _db.SaveChangesAsync(); }
            catch (DbUpdateException ex) when (DbErrors.IsUniqueViolation(ex))
            {
                return ServiceResult<CopyDto>.Fail(409, $"Accession number {accession} is already used by another copy.");
            }

            await _audit.LogAsync(userId, "INSERT", "BookCopies", copy.CopyId.ToString(), $"BookId={bookId}; AccessionNo={accession}");
            var created = await GetAsync(copy.CopyId);
            return ServiceResult<CopyDto>.Ok(created.Value!, 201);
        }

        public async Task<ServiceResult<CopyDto>> UpdateAsync(int copyId, CopyUpdateDto dto, int? userId)
        {
            var copy = await _db.BookCopies.FirstOrDefaultAsync(c => c.CopyId == copyId);
            if (copy == null) return ServiceResult<CopyDto>.Fail(404, $"Copy {copyId} was not found.");

            var accession = (dto.AccessionNo ?? "").Trim();
            var problem = ValidateAccession(accession);
            if (problem != null) return ServiceResult<CopyDto>.Fail(400, problem);

            if (await _db.BookCopies.AnyAsync(c => c.AccessionNo == accession && c.CopyId != copyId))
                return ServiceResult<CopyDto>.Fail(409, $"Accession number {accession} is already used by another copy.");

            var old = copy.AccessionNo;
            copy.AccessionNo = accession;
            try { await _db.SaveChangesAsync(); }
            catch (DbUpdateException ex) when (DbErrors.IsUniqueViolation(ex))
            {
                return ServiceResult<CopyDto>.Fail(409, $"Accession number {accession} is already used by another copy.");
            }

            await _audit.LogAsync(userId, "UPDATE", "BookCopies", copyId.ToString(), $"AccessionNo {old} -> {accession}");
            return await GetAsync(copyId);
        }

        public async Task<ServiceResult<CopyDto>> ChangeStatusAsync(int copyId, CopyStatusDto dto, int? userId)
        {
            var status = CopyStatuses.All.FirstOrDefault(s => string.Equals(s, dto.Status?.Trim(), StringComparison.OrdinalIgnoreCase));
            if (status == null)
                return ServiceResult<CopyDto>.Fail(400, $"Status must be one of: {string.Join(", ", CopyStatuses.All)}.");

            var remarks = string.IsNullOrWhiteSpace(dto.Remarks) ? null : dto.Remarks.Trim();
            if (remarks != null && remarks.Length > 255)
                return ServiceResult<CopyDto>.Fail(400, "Remarks must be at most 255 characters.");

            var copy = await _db.BookCopies.FirstOrDefaultAsync(c => c.CopyId == copyId);
            if (copy == null) return ServiceResult<CopyDto>.Fail(404, $"Copy {copyId} was not found.");

            if (status != CopyStatuses.Available && remarks == null)
                return ServiceResult<CopyDto>.Fail(400, $"Remarks are required when marking a copy as {status}.");

            // A copy that is out on loan cannot be withdrawn from the collection.
            if (status == CopyStatuses.Withdrawn && await _db.Loans.AnyAsync(l => l.CopyId == copyId && l.ReturnDate == null))
                return ServiceResult<CopyDto>.Fail(409, "This copy is currently on loan and cannot be withdrawn. Receive the return first.");

            var oldStatus = copy.CopyStatus;
            copy.CopyStatus = status;

            if (status == CopyStatuses.Withdrawn)
            {
                if (copy.WithdrawnOn == null)   // record who/when only on the first withdrawal
                {
                    copy.WithdrawnOn = DateOnly.FromDateTime(DateTime.Now);
                    copy.WithdrawnBy = userId;
                }
                copy.ReviewRemarks = remarks;
            }
            else
            {
                // DB rule: WithdrawnOn/WithdrawnBy must be empty unless the copy is Withdrawn
                copy.WithdrawnOn = null;
                copy.WithdrawnBy = null;
                copy.ReviewRemarks = status == CopyStatuses.Available ? null : remarks;
            }

            await _db.SaveChangesAsync();
            await _audit.LogAsync(userId, "UPDATE", "BookCopies", copyId.ToString(),
                $"Status {oldStatus} -> {status}; remarks={remarks}");
            return await GetAsync(copyId);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int copyId, int? userId)
        {
            var copy = await _db.BookCopies.FirstOrDefaultAsync(c => c.CopyId == copyId);
            if (copy == null) return ServiceResult<bool>.Fail(404, $"Copy {copyId} was not found.");

            if (await _db.Loans.AnyAsync(l => l.CopyId == copyId))
                return ServiceResult<bool>.Fail(409, "This copy has loan history and cannot be deleted. Mark it as Withdrawn instead.");

            var accession = copy.AccessionNo;
            _db.BookCopies.Remove(copy);
            await _db.SaveChangesAsync();
            await _audit.LogAsync(userId, "DELETE", "BookCopies", copyId.ToString(), $"AccessionNo={accession}");
            return ServiceResult<bool>.Ok(true, 204);
        }

        private static string? ValidateAccession(string accession)
        {
            if (accession.Length == 0) return "Accession number is required.";
            if (accession.Length > 30) return "Accession number must be at most 30 characters.";
            return null;
        }
    }
}
