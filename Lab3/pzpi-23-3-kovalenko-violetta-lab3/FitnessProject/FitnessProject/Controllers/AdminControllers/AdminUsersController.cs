using FitnessProject.BLL.DTO.User;
using FitnessProject.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitnessProject.Resources;
using Microsoft.Extensions.Localization;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminUsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public AdminUsersController(IUserService userService, IStringLocalizer<SharedResources> localizer)
    {
        _userService = userService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound(new { error = _localizer["Errors.NotFound"] });
        return Ok(user);
    }

    [HttpGet("{id}/details")]
    public async Task<ActionResult<UserDetailsDto>> GetDetails(int id)
    {
        var user = await _userService.GetUserDetailsByIdAsync(id);
        if (user == null)
            return NotFound(new { error = _localizer["Errors.NotFound"] });
        return Ok(user);
    }
}

