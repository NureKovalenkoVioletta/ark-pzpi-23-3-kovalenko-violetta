using FitnessProject.BLL.DTO.Localization;
using FitnessProject.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/admin/localization")]
[Authorize(Roles = "SuperAdmin")]
public class AdminLocalizationController : ControllerBase
{
    private readonly ILocalizationAdminService _service;

    public AdminLocalizationController(ILocalizationAdminService service)
    {
        _service = service;
    }

    [HttpGet("keys")]
    public async Task<ActionResult<Dictionary<string, string>>> GetKeys([FromQuery] string culture = "uk")
    {
        var keys = await _service.GetKeysAsync(culture);
        return Ok(keys);
    }

    [HttpGet("missing")]
    public async Task<ActionResult<IEnumerable<string>>> GetMissing([FromQuery] string @base = "uk", [FromQuery] string compare = "en")
    {
        var missing = await _service.GetMissingKeysAsync(@base, compare);
        return Ok(missing);
    }

    [HttpPut("keys")]
    public async Task<IActionResult> UpdateKey([FromBody] LocalizationKeyUpdateDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Key) || string.IsNullOrWhiteSpace(dto.Culture))
        {
            return BadRequest(new { error = "Invalid payload" });
        }

        await _service.UpdateKeyAsync(dto);
        return NoContent();
    }

    [HttpPost("export")]
    public async Task<ActionResult<LocalizationExportDto>> Export()
    {
        var payload = await _service.ExportAsync();
        return Ok(payload);
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import([FromBody] LocalizationImportDto dto)
    {
        if (dto == null || (dto.Uk == null && dto.En == null))
        {
            return BadRequest(new { error = "Invalid payload" });
        }

        await _service.ImportAsync(dto);
        return NoContent();
    }
}

