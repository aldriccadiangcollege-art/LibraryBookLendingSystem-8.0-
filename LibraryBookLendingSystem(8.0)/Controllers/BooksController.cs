using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LibraryBookLendingSystem_8._0_.Models.DTOs.Books;
using LibraryBookLendingSystem_8._0_.Security;
using LibraryBookLendingSystem_8._0_.Services.Interfaces;

namespace LibraryBookLendingSystem_8._0_.Controllers
{
    [ApiController]
    [Route("api/books")]
    [Authorize]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _books;
        public BooksController(IBookService books) => _books = books;

        [HttpGet]
        [Authorize(Roles = CatalogRoles.Read)]
        public async Task<IActionResult> GetAll([FromQuery] string? term) =>
            this.ToActionResult(await _books.GetAllAsync(term));

        [HttpGet("{id:int}")]
        [Authorize(Roles = CatalogRoles.Read)]
        public async Task<IActionResult> Get(int id) =>
            this.ToActionResult(await _books.GetAsync(id));

        [HttpPost]
        [Authorize(Roles = CatalogRoles.Write)]
        public async Task<IActionResult> Create([FromBody] BookUpsertDto dto) =>
            this.ToActionResult(await _books.CreateAsync(dto, User.GetUserId()));

        [HttpPut("{id:int}")]
        [Authorize(Roles = CatalogRoles.Write)]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpsertDto dto) =>
            this.ToActionResult(await _books.UpdateAsync(id, dto, User.GetUserId()));

        [HttpDelete("{id:int}")]
        [Authorize(Roles = CatalogRoles.Admin)]
        public async Task<IActionResult> Delete(int id) =>
            this.ToActionResult(await _books.DeleteAsync(id, User.GetUserId()));

        /// <summary>Download the whole catalog as CSV (opens in Excel).</summary>
        [HttpGet("export")]
        [Authorize(Roles = CatalogRoles.Read)]
        public async Task<IActionResult> Export()
        {
            var result = await _books.ExportCsvAsync(User.GetUserId());
            return File(result.Value!, "text/csv", $"books-{DateTime.Now:yyyyMMdd-HHmm}.csv");
        }

        /// <summary>Upload a CSV of books. 400 = invalid rows, 409 = duplicate ISBN; nothing is saved on error.</summary>
        [HttpPost("import")]
        [Authorize(Roles = CatalogRoles.Write)]
        [RequestSizeLimit(2_000_000)]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Problem(detail: "Choose a CSV file to upload.", statusCode: 400, title: "Bad request");
            if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
                return Problem(detail: "Only .csv files are accepted.", statusCode: 400, title: "Bad request");

            await using var stream = file.OpenReadStream();
            var result = await _books.ImportCsvAsync(stream, User.GetUserId());
            // success and failure both return the ImportResult body (message + row-level errors)
            return StatusCode(result.StatusCode, result.Value);
        }
    }
}
