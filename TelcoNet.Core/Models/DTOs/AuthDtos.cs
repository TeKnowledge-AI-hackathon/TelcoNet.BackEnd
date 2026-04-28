using System.ComponentModel.DataAnnotations;

namespace TelcoNet.Core.Models.DTOs;

// ── Auth DTOs ──

public class LoginRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequestDto
{
    [Required]
    [MinLength(3, ErrorMessage = "Full name must be at least 3 characters")]
    public string FullName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    public string Password { get; set; } = string.Empty;
    
    public string Role { get; set; } = "Viewer";
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class CreateUserDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // e.g. "Admin", "Operator", "Viewer"
}

