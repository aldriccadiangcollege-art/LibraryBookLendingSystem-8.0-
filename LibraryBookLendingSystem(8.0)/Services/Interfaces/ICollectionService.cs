using LibraryBookLendingSystem_8._0_.Models.DTOs.Books;

namespace LibraryBookLendingSystem.Services.Interfaces
{
    public interface ICollectionService
    {
        /// <param name="filter">null, outdated, duplicates, damaged, lost or withdrawn</param>
        Task<ServiceResult<CollectionReviewDto>> GetReviewAsync(string? filter);
    }
}
