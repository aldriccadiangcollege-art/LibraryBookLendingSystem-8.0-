using System.Text;
using Microsoft.EntityFrameworkCore;
using LibraryBookLendingSystem_8._0_.Data;
using LibraryBookLendingSystem_8._0_.Models.DTOs.Books;
using LibraryBookLendingSystem_8._0_.Models.Entities;
using LibraryBookLendingSystem_8._0_.Services.Interfaces;
using LibraryBookLendingSystem_8._0_.Validators;


namespace LibraryBookLendingSystem_8._0_.Services.Implementations
{
    public class BookService : IBookService
    {
        private static readonly string[] CsvHeaders =
            { "Isbn", "Title", "Author", "Publisher", "PublicationYear", "Category", "ShelfLocation", "IsDigitalResource", "AccessUrl" };

        private const int MaxImportRows = 2000;

        private readonly LibraryDbContext _db;
        private readonly IAuditService _audit;

        public BookService(LibraryDbContext db, IAuditService audit)
        {
            _db = db;
            _audit = audit;
        }

        // ---------- queries ----------

        private static IQueryable<Book> Filter(IQueryable<Book> q, string? term)
        {
            if (string.IsNullOrWhiteSpace(term)) return q;
            term = term.Trim();
            return q.Where(b => b.Title.Contains(term)
                             || b.Author.Contains(term)
                             || b.Category.Contains(term)
                             || (b.Isbn != null && b.Isbn.Contains(term)));
        }

        private static IQueryable<BookDto> ToDto(IQueryable<Book> q) =>
            q.Select(b => new BookDto
            {
                BookId = b.BookId,
                Isbn = b.Isbn,
                Title = b.Title,
                Author = b.Author,
                Publisher = b.Publisher,
                PublicationYear = b.PublicationYear,
                Category = b.Category,
                ShelfLocation = b.ShelfLocation,
                IsDigitalResource = b.IsDigitalResource,
                AccessUrl = b.AccessUrl,
                CreatedAt = b.CreatedAt,
                TotalCopies = b.BookCopies.Count(),
                AvailableNow = b.BookCopies.Count(c => c.CopyStatus == "Available" && !c.Loans.Any(l => l.ReturnDate == null))
            });

        public async Task<ServiceResult<List<BookDto>>> GetAllAsync(string? term)
        {
            var list = await ToDto(Filter(_db.Books.AsNoTracking(), term).OrderBy(b => b.Title)).ToListAsync();
            return ServiceResult<List<BookDto>>.Ok(list);
        }

        public async Task<ServiceResult<BookDto>> GetAsync(int id)
        {
            var dto = await ToDto(_db.Books.AsNoTracking().Where(b => b.BookId == id)).FirstOrDefaultAsync();
            return dto == null
                ? ServiceResult<BookDto>.Fail(404, $"Book {id} was not found.")
                : ServiceResult<BookDto>.Ok(dto);
        }

        public async Task<ServiceResult<List<CatalogBookDto>>> SearchAsync(string? term)
        {
            var list = await Filter(_db.Books.AsNoTracking(), term)
                .OrderBy(b => b.Title)
                .Select(b => new CatalogBookDto
                {
                    BookId = b.BookId,
                    Isbn = b.Isbn,
                    Title = b.Title,
                    Author = b.Author,
                    Category = b.Category,
                    PublicationYear = b.PublicationYear,
                    ShelfLocation = b.ShelfLocation,
                    IsDigitalResource = b.IsDigitalResource,
                    AccessUrl = b.AccessUrl,
                    TotalCopies = b.BookCopies.Count(),
                    AvailableNow = b.BookCopies.Count(c => c.CopyStatus == "Available" && !c.Loans.Any(l => l.ReturnDate == null))
                })
                .ToListAsync();
            return ServiceResult<List<CatalogBookDto>>.Ok(list);
        }

        // ---------- create / update / delete ----------

        public async Task<ServiceResult<BookDto>> CreateAsync(BookUpsertDto dto, int? userId)
        {
            BookValidator.Normalize(dto);
            var errors = BookValidator.Validate(dto);
            if (errors.Count > 0)
                return ServiceResult<BookDto>.Fail(400, string.Join(" ", errors.Select(e => e.Message)));

            if (dto.Isbn != null && await _db.Books.AnyAsync(b => b.Isbn == dto.Isbn))
                return ServiceResult<BookDto>.Fail(409, $"A book with ISBN {dto.Isbn} already exists.");

            var book = new Book();
            Apply(book, dto);
            _db.Books.Add(book);

            try { await _db.SaveChangesAsync(); }
            catch (DbUpdateException ex) when (DbErrors.IsUniqueViolation(ex))
            {
                return ServiceResult<BookDto>.Fail(409, $"A book with ISBN {dto.Isbn} already exists.");
            }

            await _audit.LogAsync(userId, "INSERT", "Books", book.BookId.ToString(), $"Title={book.Title}; Isbn={book.Isbn}");
            var created = await GetAsync(book.BookId);
            return ServiceResult<BookDto>.Ok(created.Value!, 201);
        }

        public async Task<ServiceResult<BookDto>> UpdateAsync(int id, BookUpsertDto dto, int? userId)
        {
            var book = await _db.Books.FirstOrDefaultAsync(b => b.BookId == id);
            if (book == null) return ServiceResult<BookDto>.Fail(404, $"Book {id} was not found.");

            BookValidator.Normalize(dto);
            var errors = BookValidator.Validate(dto);
            if (errors.Count > 0)
                return ServiceResult<BookDto>.Fail(400, string.Join(" ", errors.Select(e => e.Message)));

            if (dto.Isbn != null && await _db.Books.AnyAsync(b => b.Isbn == dto.Isbn && b.BookId != id))
                return ServiceResult<BookDto>.Fail(409, $"Another book with ISBN {dto.Isbn} already exists.");

            if (dto.IsDigitalResource && !book.IsDigitalResource && await _db.BookCopies.AnyAsync(c => c.BookId == id))
                return ServiceResult<BookDto>.Fail(409, "This book has physical copies, so it cannot be changed to a digital resource.");

            Apply(book, dto);
            try { await _db.SaveChangesAsync(); }
            catch (DbUpdateException ex) when (DbErrors.IsUniqueViolation(ex))
            {
                return ServiceResult<BookDto>.Fail(409, $"Another book with ISBN {dto.Isbn} already exists.");
            }

            await _audit.LogAsync(userId, "UPDATE", "Books", id.ToString(), $"Title={book.Title}; Isbn={book.Isbn}");
            return await GetAsync(id);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id, int? userId)
        {
            var book = await _db.Books.FirstOrDefaultAsync(b => b.BookId == id);
            if (book == null) return ServiceResult<bool>.Fail(404, $"Book {id} was not found.");

            // Copies (and their loan history) must never be wiped by deleting a title.
            if (await _db.BookCopies.AnyAsync(c => c.BookId == id))
                return ServiceResult<bool>.Fail(409, "This book still has copies. Withdraw its copies instead of deleting the title.");

            var title = book.Title;
            _db.Books.Remove(book);
            await _db.SaveChangesAsync();
            await _audit.LogAsync(userId, "DELETE", "Books", id.ToString(), $"Title={title}");
            return ServiceResult<bool>.Ok(true, 204);
        }

        private static void Apply(Book book, BookUpsertDto dto)
        {
            book.Isbn = dto.Isbn;
            book.Title = dto.Title;
            book.Author = dto.Author;
            book.Publisher = dto.Publisher;
            book.PublicationYear = dto.PublicationYear;
            book.Category = dto.Category;
            book.ShelfLocation = dto.ShelfLocation;
            book.IsDigitalResource = dto.IsDigitalResource;
            book.AccessUrl = dto.AccessUrl;
        }

        // ---------- CSV ----------

        public async Task<ServiceResult<byte[]>> ExportCsvAsync(int? userId)
        {
            var books = await _db.Books.AsNoTracking().OrderBy(b => b.BookId).ToListAsync();

            var sb = new StringBuilder();
            sb.Append(string.Join(",", CsvHeaders)).Append("\r\n");
            foreach (var b in books)
            {
                sb.Append(string.Join(",", new[]
                {
                    CsvUtil.Escape(b.Isbn),
                    CsvUtil.Escape(b.Title),
                    CsvUtil.Escape(b.Author),
                    CsvUtil.Escape(b.Publisher),
                    b.PublicationYear?.ToString() ?? "",
                    CsvUtil.Escape(b.Category),
                    CsvUtil.Escape(b.ShelfLocation),
                    b.IsDigitalResource ? "true" : "false",
                    CsvUtil.Escape(b.AccessUrl)
                })).Append("\r\n");
            }

            // UTF-8 with BOM so Excel shows Filipino characters correctly.
            var bytes = new UTF8Encoding(true).GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            await _audit.LogAsync(userId, "EXPORT", "Books", null, $"rows={books.Count}");
            return ServiceResult<byte[]>.Ok(bytes);
        }

        /// <summary>
        /// All-or-nothing import. Any invalid row: 400 with row-level errors. Only duplicate ISBNs
        /// (already in the database, or repeated in the file): 409. Nothing is saved unless every row is valid.
        /// </summary>
        public async Task<ServiceResult<ImportResultDto>> ImportCsvAsync(Stream csv, int? userId)
        {
            string text;
            using (var reader = new StreamReader(csv, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
                text = await reader.ReadToEndAsync();

            var rows = CsvUtil.Parse(text);
            var result = new ImportResultDto();

            if (rows.Count == 0)
                return await RejectAsync(400, result, "The file is empty.", userId);

            var columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < rows[0].Count; i++) columns.TryAdd(rows[0][i].Trim(), i);

            var missing = new[] { "Title", "Author", "Category" }.Where(c => !columns.ContainsKey(c)).ToList();
            if (missing.Count > 0)
                return await RejectAsync(400, result, $"Missing required column(s): {string.Join(", ", missing)}. Expected header: {string.Join(",", CsvHeaders)}", userId);

            // keep the spreadsheet row number with each data row; skip fully blank lines
            var dataRows = new List<(int Row, List<string> Cells)>();
            for (var i = 1; i < rows.Count; i++)
                if (rows[i].Any(c => !string.IsNullOrWhiteSpace(c))) dataRows.Add((i + 1, rows[i]));

            result.TotalRows = dataRows.Count;
            if (dataRows.Count == 0)
                return await RejectAsync(400, result, "The file has a header but no data rows.", userId);
            if (dataRows.Count > MaxImportRows)
                return await RejectAsync(400, result, $"Too many rows ({dataRows.Count}). The limit is {MaxImportRows} per file.", userId);

            string Cell(List<string> cells, string name) =>
                columns.TryGetValue(name, out var idx) && idx < cells.Count ? cells[idx] : "";

            var existingIsbns = (await _db.Books.Where(b => b.Isbn != null).Select(b => b.Isbn!).ToListAsync())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var seenInFile = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var valid = new List<BookUpsertDto>();

            foreach (var (row, cells) in dataRows)
            {
                var rowErrors = new List<RowErrorDto>();
                var dto = new BookUpsertDto
                {
                    Isbn = Cell(cells, "Isbn"),
                    Title = Cell(cells, "Title"),
                    Author = Cell(cells, "Author"),
                    Publisher = Cell(cells, "Publisher"),
                    Category = Cell(cells, "Category"),
                    ShelfLocation = Cell(cells, "ShelfLocation"),
                    AccessUrl = Cell(cells, "AccessUrl")
                };

                var yearText = Cell(cells, "PublicationYear").Trim();
                if (yearText.Length > 0)
                {
                    if (short.TryParse(yearText, out var year)) dto.PublicationYear = year;
                    else rowErrors.Add(new RowErrorDto { Row = row, Field = "PublicationYear", Message = $"'{yearText}' is not a valid year." });
                }

                var digitalText = Cell(cells, "IsDigitalResource").Trim().ToLowerInvariant();
                switch (digitalText)
                {
                    case "": case "false": case "0": case "no": dto.IsDigitalResource = false; break;
                    case "true": case "1": case "yes": dto.IsDigitalResource = true; break;
                    default:
                        rowErrors.Add(new RowErrorDto { Row = row, Field = "IsDigitalResource", Message = $"'{digitalText}' must be true or false." });
                        break;
                }

                BookValidator.Normalize(dto);
                foreach (var e in BookValidator.Validate(dto))
                    rowErrors.Add(new RowErrorDto { Row = row, Field = e.Field, Message = e.Message });

                if (dto.Isbn != null)
                {
                    if (existingIsbns.Contains(dto.Isbn))
                        rowErrors.Add(new RowErrorDto { Row = row, Field = "Isbn", Code = "duplicate-isbn", Message = $"ISBN {dto.Isbn} already exists in the catalog." });
                    else if (seenInFile.TryGetValue(dto.Isbn, out var firstRow))
                        rowErrors.Add(new RowErrorDto { Row = row, Field = "Isbn", Code = "duplicate-isbn", Message = $"ISBN {dto.Isbn} is repeated (first used on row {firstRow})." });
                    else
                        seenInFile[dto.Isbn] = row;
                }

                result.Errors.AddRange(rowErrors);
                if (rowErrors.Count == 0) valid.Add(dto);
            }

            if (result.Errors.Count > 0)
            {
                var onlyDuplicates = result.Errors.All(e => e.Code == "duplicate-isbn");
                return await RejectAsync(onlyDuplicates ? 409 : 400, result,
                    onlyDuplicates
                        ? "Import rejected: duplicate ISBN(s) found. No books were imported."
                        : "Import rejected: some rows are invalid. No books were imported.", userId);
            }

            _db.Books.AddRange(valid.Select(d => { var b = new Book(); Apply(b, d); return b; }));
            try { await _db.SaveChangesAsync(); }
            catch (DbUpdateException ex) when (DbErrors.IsUniqueViolation(ex))
            {
                return await RejectAsync(409, result, "Import rejected: a duplicate ISBN was added while importing. No books were imported.", userId);
            }

            result.Imported = valid.Count;
            result.Message = $"Imported {valid.Count} book(s).";
            await _audit.LogAsync(userId, "IMPORT", "Books", null, $"rows={result.TotalRows}; imported={result.Imported}");
            return ServiceResult<ImportResultDto>.Ok(result);
        }

        private async Task<ServiceResult<ImportResultDto>> RejectAsync(int status, ImportResultDto result, string message, int? userId)
        {
            result.Message = message;
            result.Imported = 0;
            // drop any books queued for insert so the audit save cannot re-send them
            _db.ChangeTracker.Clear();
            await _audit.LogAsync(userId, "IMPORT", "Books", null,
                $"rejected; rows={result.TotalRows}; errors={result.Errors.Count}; {message}");
            return ServiceResult<ImportResultDto>.Fail(status, message, result);
        }
    }
}
