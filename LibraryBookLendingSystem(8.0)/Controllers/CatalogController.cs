using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LibraryBookLendingSystem_8._0_.Security;
using LibraryBookLendingSystem_8._0_.Services.Interfaces;

namespace LibraryBookLendingSystem_8._0_.Controllers
{
    [ApiController]
    [Route("api/catalog")]
    public class CatalogController : ControllerBase
    {
        private readonly IBookService _books;
        public CatalogController(IBookService books) => _books = books;

        /// <summary>Keyword search (title, author, category, ISBN) with availability. Any logged-in role, including Student.</summary>
        [HttpGet("search")]
        [Authorize(Roles = CatalogRoles.Everyone)]
        public async Task<IActionResult> Search([FromQuery] string? term) =>
            this.ToActionResult(await _books.SearchAsync(term));
    }
}
