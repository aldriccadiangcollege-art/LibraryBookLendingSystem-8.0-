using LibraryBookLendingSystem_8._0_.Models.DTOs.Books;

namespace LibraryBookLendingSystem_8._0_.Services.Interfaces
{
    public interface IBookService
    {
        Task<ServiceResult<List<BookDto>>> GetAllAsync(string? term);
        Task<ServiceResult<BookDto>> GetAsync(int id);
        Task<ServiceResult<BookDto>> CreateAsync(BookUpsertDto dto, int? userId);
        Task<ServiceResult<BookDto>> UpdateAsync(int id, BookUpsertDto dto, int? userId);
        Task<ServiceResult<bool>> DeleteAsync(int id, int? userId);
        Task<ServiceResult<List<CatalogBookDto>>> SearchAsync(string? term);
        Task<ServiceResult<byte[]>> ExportCsvAsync(int? userId);
        Task<ServiceResult<ImportResultDto>> ImportCsvAsync(Stream csv, int? userId);
    }
}
