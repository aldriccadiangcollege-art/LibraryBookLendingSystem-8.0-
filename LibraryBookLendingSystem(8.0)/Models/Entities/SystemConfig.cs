namespace LibraryBookLendingSystem.Models.Entities
{
    public class SystemConfig
    {
        public string ConfigKey { get; set; } = null!;
        public string ConfigValue { get; set; } = null!;
        public string? Description { get; set; }
    }
}