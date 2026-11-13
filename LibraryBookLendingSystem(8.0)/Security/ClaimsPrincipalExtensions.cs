using System.Security.Claims;

namespace LibraryBookLendingSystem_8._0_.Security
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>Reads the logged-in user's Users.UserId from the JWT (NameIdentifier or "sub").</summary>
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
            return int.TryParse(value, out var id) ? id : null;
        }
    }
}
