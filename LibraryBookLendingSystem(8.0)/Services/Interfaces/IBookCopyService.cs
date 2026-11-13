using LibraryBookLendingSystem_8._0_.Models.DTOs.Books;

namespace LibraryBookLendingSystem_8._0_.Services.Interfaces
{
    public interface IBookCopyService
    {
        Task<ServiceResult<List<CopyDto>>> GetByBookAsync(int bookId);
        Task<ServiceResult<CopyDto>> GetAsync(int copyId);
        Task<ServiceResult<CopyDto>> CreateAsync(int bookId, CopyCreateDto dto, int? userId);
        Task<ServiceResult<CopyDto>> UpdateAsync(int copyId, CopyUpdateDto dto, int? userId);
        Task<ServiceResult<CopyDto>> ChangeStatusAsync(int copyId, CopyStatusDto dto, int? userId);
        Task<ServiceResult<bool>> DeleteAsync(int copyId, int? userId);
    }
}
