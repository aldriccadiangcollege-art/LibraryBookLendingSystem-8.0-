using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LibraryBookLendingSystem_8._0_.Security;
using LibraryBookLendingSystem.Services.Interfaces;

namespace LibraryBookLendingSystem_8._0_.Controllers
{
    [ApiController]
    [Route("api/collection")]
    public class CollectionController : ControllerBase
    {
        private readonly ICollectionService _collection;
        public CollectionController(ICollectionService collection) => _collection = collection;

        /// <summary>Collection review: outdated (10+ years), duplicate titles, damaged/lost/withdrawn counts. Optional ?filter=</summary>
        [HttpGet("review")]
        [Authorize(Roles = CatalogRoles.Read)]
        public async Task<IActionResult> Review([FromQuery] string? filter) =>
            this.ToActionResult(await _collection.GetReviewAsync(filter));
    }
}
