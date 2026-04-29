using System.ComponentModel.DataAnnotations;

namespace TelcoNet.Data.Entities;

public class SystemSetting
{
    [Key]
    public string Key { get; set; } = string.Empty;
    
    [Required]
    public string Value { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public string Group { get; set; } = "General"; // e.g., Security, Data, Appearance
}
