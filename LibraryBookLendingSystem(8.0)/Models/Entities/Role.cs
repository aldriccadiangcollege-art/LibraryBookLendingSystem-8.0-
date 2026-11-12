using LibraryBookLendingSystem_8._0_.Models.Entities;
using System.Collections.Generic;

namespace LibraryBookLendingSystem.Models.Entities
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}