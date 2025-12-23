using FitnessProject.BLL.DTO.Device;
using FitnessProject.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitnessProject.Resources;
using Microsoft.Extensions.Localization;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/admin/devices")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminDevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public AdminDevicesController(IDeviceService deviceService, IStringLocalizer<SharedResources> localizer)
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
}

