using Microsoft.AspNetCore.Mvc;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.BLL.DTO.Device;
using FitnessProject.Resources;
using Microsoft.Extensions.Localization;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public DevicesController(IDeviceService deviceService, IStringLocalizer<SharedResources> localizer)
    {
        _deviceService = deviceService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeviceResponseDto>>> GetAll()
    {
        var devices = await _deviceService.GetAllAsync();
        return Ok(devices);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DeviceResponseDto>> GetById(int id)
    {
        var device = await _deviceService.GetByIdAsync(id);
        if (device == null)
            return NotFound(new { error = _localizer["Errors.NotFound"] });

        return Ok(device);
    }

    [HttpGet("{id}/details")]
    public async Task<ActionResult<DeviceDetailsDto>> GetDetailsById(int id)
    {
        var device = await _deviceService.GetDeviceDetailsByIdAsync(id);
        if (device == null)
            return NotFound(new { error = _localizer["Errors.NotFound"] });

        return Ok(device);
    }

    [HttpPost]
    public async Task<ActionResult<DeviceResponseDto>> Create([FromBody] DeviceCreateDto createDto)
    {
        try
        {
            var created = await _deviceService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = created.DeviceId }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = _localizer["Errors.BadRequest"], details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DeviceResponseDto>> Update(int id, [FromBody] DeviceUpdateDto updateDto)
    {
        if (id != updateDto.DeviceId)
            return BadRequest(new { error = _localizer["Errors.IdMismatch"] });

        try
        {
            var updated = await _deviceService.UpdateAsync(updateDto);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = _localizer["Errors.NotFound"], details = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = _localizer["Errors.BadRequest"], details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _deviceService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { error = _localizer["Errors.NotFound"] });

        return NoContent();
    }
}

