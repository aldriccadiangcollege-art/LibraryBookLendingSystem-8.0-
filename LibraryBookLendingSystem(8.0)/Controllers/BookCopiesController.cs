using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LibraryBookLendingSystem_8._0_.Security;
using LibraryBookLendingSystem_8._0_.Services.Interfaces;
using LibraryBookLendingSystem_8._0_.Models.DTOs.Books;

namespace LibraryBookLendingSystem_8._0_.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class BookCopiesController : ControllerBase
    {
        private readonly IBookCopyService _copies;
        public BookCopiesController(IBookCopyService copies) => _copies = copies;

        [HttpGet("books/{bookId:int}/copies")]
        [Authorize(Roles = CatalogRoles.Read)]
        public async Task<IActionResult> GetByBook(int bookId) =>
            this.ToActionResult(await _copies.GetByBookAsync(bookId));

        [HttpPost("books/{bookId:int}/copies")]
        [Authorize(Roles = CatalogRoles.Write)]
        public async Task<IActionResult> Create(int bookId, [FromBody] CopyCreateDto dto) =>
            this.ToActionResult(await _copies.CreateAsync(bookId, dto, User.GetUserId()));

        [HttpGet("copies/{id:int}")]
        [Authorize(Roles = CatalogRoles.Read)]
        public async Task<IActionResult> Get(int id) =>
            this.ToActionResult(await _copies.GetAsync(id));

        [HttpPut("copies/{id:int}")]
        [Authorize(Roles = CatalogRoles.Write)]
        public async Task<IActionResult> Update(int id, [FromBody] CopyUpdateDto dto) =>
            this.ToActionResult(await _copies.UpdateAsync(id, dto, User.GetUserId()));

        /// <summary>Available / Damaged / Lost / Withdrawn. Remarks required unless Available; withdrawal blocked while on loan.</summary>
        [HttpPatch("copies/{id:int}/status")]
        [Authorize(Roles = CatalogRoles.Write)]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] CopyStatusDto dto) =>
            this.ToActionResult(await _copies.ChangeStatusAsync(id, dto, User.GetUserId()));

        [HttpDelete("copies/{id:int}")]
        [Authorize(Roles = CatalogRoles.Admin)]
        public async Task<IActionResult> Delete(int id) =>
            this.ToActionResult(await _copies.DeleteAsync(id, User.GetUserId()));
    }
}
