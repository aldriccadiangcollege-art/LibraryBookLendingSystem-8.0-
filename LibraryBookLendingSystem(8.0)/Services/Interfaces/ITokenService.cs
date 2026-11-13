using LibraryBookLendingSystem.Models.Entities;

namespace LibraryBookLendingSystem.Services.Interfaces;

public interface ITokenService
{
    string CreateToken(User user, IEnumerable<string> roles);
}