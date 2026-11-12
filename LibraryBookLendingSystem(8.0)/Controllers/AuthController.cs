using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryBookLendingSystem.Data;
using LibraryBookLendingSystem.Models.DTOs.Auth;
using LibraryBookLendingSystem.Services;

namespace LibraryBookLendingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly LibraryDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthController(LibraryDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            var roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList();
            var token = _tokenService.CreateToken(user, roles);

            return Ok(new
            {
                username = user.Username,
                email = user.Email,
                token = token,
                roles = roles
            });
        }
    }
}