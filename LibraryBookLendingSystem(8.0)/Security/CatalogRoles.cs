namespace LibraryBookLendingSystem_8._0_.Security
{
    /// <summary>Role groups used by the Member 2 endpoints (names match the Roles table seed).</summary>
    public static class CatalogRoles
    {
        public const string Read = "Librarian,LR Coordinator,Viewer";
        public const string Write = "Librarian,LR Coordinator";
        public const string Admin = "Librarian";
        public const string Everyone = "Librarian,LR Coordinator,Viewer,Student";
    }
}
