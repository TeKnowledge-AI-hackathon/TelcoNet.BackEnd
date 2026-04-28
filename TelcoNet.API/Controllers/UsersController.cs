using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TelcoNet.Core.Models.DTOs;
using TelcoNet.Data;
using TelcoNet.Data.Entities;

namespace TelcoNet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Get all users (Admin only).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.Users
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role.ToString(),
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                IsActive = u.IsActive
            }).ToListAsync();

        return Ok(users);
    }

    /// <summary>Create a new user and assign a role (Admin only).</summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto request)
    {
        var exists = await _db.Users.AnyAsync(u => u.Email == request.Email);
        if (exists) return Conflict(new { error = "User with this email already exists." });

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            return BadRequest(new { error = "Invalid role. Use: Viewer, Operator, or Admin." });

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        });
    }

    /// <summary>Get a specific user by ID (Admin only).</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound(new { error = "User not found." });

        return Ok(new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            IsActive = user.IsActive
        });
    }

    /// <summary>Update a user's role (Admin only).</summary>
    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto request)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound(new { error = "User not found." });

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            return BadRequest(new { error = "Invalid role. Use: Viewer, Operator, or Admin." });

        user.Role = role;
        await _db.SaveChangesAsync();

        return Ok(new { message = $"User role updated to {role}." });
    }

    /// <summary>Deactivate a user (Admin only).</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound(new { error = "User not found." });

        user.IsActive = false;
        await _db.SaveChangesAsync();

        return Ok(new { message = "User deactivated." });
    }
}
