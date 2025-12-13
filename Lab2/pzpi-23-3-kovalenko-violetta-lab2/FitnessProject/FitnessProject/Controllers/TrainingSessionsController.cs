using Microsoft.AspNetCore.Mvc;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.BLL.DTO.TrainingSession;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrainingSessionsController : ControllerBase
{
    private readonly ITrainingSessionService _trainingSessionService;

    public TrainingSessionsController(ITrainingSessionService trainingSessionService)
    {
        _trainingSessionService = trainingSessionService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrainingSessionResponseDto>>> GetAll()
    {
        var sessions = await _trainingSessionService.GetAllAsync();
        return Ok(sessions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TrainingSessionResponseDto>> GetById(int id)
    {
        var session = await _trainingSessionService.GetByIdAsync(id);
        if (session == null)
            return NotFound();

        return Ok(session);
    }

    [HttpPost]
    public async Task<ActionResult<TrainingSessionResponseDto>> Create([FromBody] TrainingSessionCreateDto createDto)
    {
        try
        {
            var created = await _trainingSessionService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = created.SessionId }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TrainingSessionResponseDto>> Update(int id, [FromBody] TrainingSessionUpdateDto updateDto)
    {
        if (id != updateDto.SessionId)
            return BadRequest(new { error = "ID mismatch" });

        try
        {
            var updated = await _trainingSessionService.UpdateAsync(updateDto);
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
        var deleted = await _trainingSessionService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}

