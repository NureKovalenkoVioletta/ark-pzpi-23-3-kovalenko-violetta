using FitnessProject.BLL.DTO.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using FitnessProject.Resources;
using Microsoft.Extensions.Localization;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/admin/overview")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminOverviewController : ControllerBase
{
    private static readonly DateTime StartedAt = DateTime.UtcNow;
    private readonly IConfiguration _configuration;
    private readonly DbContext _dbContext;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public AdminOverviewController(IConfiguration configuration, Data.ApplicationDbContext dbContext, IStringLocalizer<SharedResources> localizer)
    {
        _configuration = configuration;
        _dbContext = dbContext;
        _localizer = localizer;
    }

    [HttpGet]
    public ActionResult<AdminOverviewDto> GetBasicInfoSystem()
    {
        var connString = _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        var dbName = TryParseDbName(connString);
        var lastMigration = _dbContext.Database.GetAppliedMigrations().LastOrDefault() ?? "n/a";
        var uptime = DateTime.UtcNow - StartedAt;

        var dto = new AdminOverviewDto
        {
            BuildVersion = GetAssemblyVersion(),
            Commit = _configuration["BUILD_COMMIT"] ?? string.Empty,
            Database = dbName,
            Connection = MaskConnection(connString),
            LastMigration = lastMigration,
            Uptime = $"{(int)uptime.TotalHours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}",
            StartedAtUtc = StartedAt
        };
        return Ok(dto);
    }

    private static string GetAssemblyVersion()
    {
        var asm = System.Reflection.Assembly.GetExecutingAssembly();
        var version = asm.GetName().Version?.ToString() ?? "n/a";
        var infoVersion = asm.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
            .OfType<System.Reflection.AssemblyInformationalVersionAttribute>()
            .FirstOrDefault()?.InformationalVersion;
        return infoVersion ?? version;
    }

    private static string MaskConnection(string conn)
    {
        if (string.IsNullOrWhiteSpace(conn)) return string.Empty;
        // упрощённое маскирование: оставим Data Source/Initial Catalog, уберём пароль
        var parts = conn.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var safeParts = parts.Where(p => !p.StartsWith("Password", StringComparison.OrdinalIgnoreCase)
                                       && !p.StartsWith("Pwd", StringComparison.OrdinalIgnoreCase));
        return string.Join(";", safeParts);
    }

    private static string TryParseDbName(string conn)
    {
        if (string.IsNullOrWhiteSpace(conn)) return string.Empty;
        var parts = conn.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var db = parts.FirstOrDefault(p => p.StartsWith("Initial Catalog", StringComparison.OrdinalIgnoreCase) ||
                                           p.StartsWith("Database", StringComparison.OrdinalIgnoreCase));
        if (db != null)
        {
            var kv = db.Split('=', 2, StringSplitOptions.TrimEntries);
            if (kv.Length == 2) return kv[1];
        }
        return string.Empty;
    }
}

