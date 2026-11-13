using LibraryBookLendingSystem.Data;
using LibraryBookLendingSystem.Models.DTOs.Auth;
using LibraryBookLendingSystem.Models.Entities;
using LibraryBookLendingSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryBookLendingSystem.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthController(AppDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == dto.Username);

        if (user is null)
            return Unauthorized(new { message = "Invalid username or password." });

        if (user.LockoutUntil is not null && user.LockoutUntil > DateTime.Now)
            return StatusCode(423, new { message = "Account locked. Try again later." });

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= 5)
                user.LockoutUntil = DateTime.Now.AddMinutes(15);
            await _context.SaveChangesAsync();
            return Unauthorized(new { message = "Invalid username or password." });
        }

        user.FailedLoginCount = 0;
        user.LockoutUntil = null;
        await _context.SaveChangesAsync();

        var roles = user.UserRoles.Where(ur => ur.Role != null)
                                  .Select(ur => ur.Role!.RoleName);
        var token = _tokenService.CreateToken(user, roles);

        return Ok(new
        {
            token,
            user = new { user.UserId, user.Username, user.FullName, roles }
        });
    }
}