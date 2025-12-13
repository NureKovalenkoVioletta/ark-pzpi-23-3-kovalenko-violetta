using Microsoft.AspNetCore.Mvc;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.BLL.DTO.UserProfile;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserProfilesController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;

    public UserProfilesController(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserProfileResponseDto>>> GetAll()
    {
        var profiles = await _userProfileService.GetAllAsync();
        return Ok(profiles);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserProfileResponseDto>> GetById(int id)
    {
        var profile = await _userProfileService.GetByIdAsync(id);
        if (profile == null)
            return NotFound();

        return Ok(profile);
    }

    [HttpPost]
    public async Task<ActionResult<UserProfileResponseDto>> Create([FromBody] UserProfileCreateDto createDto)
    {
        try
        {
            var created = await _userProfileService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = created.ProfileId }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserProfileResponseDto>> Update(int id, [FromBody] UserProfileUpdateDto updateDto)
    {
        if (id != updateDto.ProfileId)
            return BadRequest(new { error = "ID mismatch" });

        try
        {
            var updated = await _userProfileService.UpdateAsync(updateDto);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _userProfileService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}

