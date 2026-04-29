using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TelcoNet.Data;
using TelcoNet.Data.Entities;

namespace TelcoNet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SettingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SettingsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var settings = await _db.SystemSettings.ToListAsync();
        return Ok(settings);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] List<SystemSetting> settings)
    {
        foreach (var setting in settings)
        {
            var existing = await _db.SystemSettings.FindAsync(setting.Key);
            if (existing != null)
            {
                existing.Value = setting.Value;
                _db.SystemSettings.Update(existing);
            }
            else
            {
                _db.SystemSettings.Add(setting);
            }
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Settings updated successfully" });
    }
}
