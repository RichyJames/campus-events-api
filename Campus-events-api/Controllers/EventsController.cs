using Campus_events_api.Data;
using Campus_events_api.Dtos;
using Campus_events_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Campus_events_api.Repositories;

namespace Campus_events_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventRepository _repository;
    private readonly ApplicationDbContext _context;

    public EventsController(IEventRepository repository, ApplicationDbContext context)
    {
        _repository = repository;
        _context = context;
        
    }

    // GET: api/events
    [HttpGet]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetAll()
    {
        var events = await _repository.GetAllAsync();

        var dtos = events.Select(e => new EventDto
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            Location = e.Location,
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            Capacity = e.Capacity,
            CategoryId = e.CategoryId,
            CategoryName = e.Category.Name
        }).ToList();

        return Ok(dtos);
    }


    // GET: api/events/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventDto>> GetById(int id)
    {
        var e = await _repository.GetByIdAsync(id);
        if (e == null)
            return NotFound();

        return new EventDto
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            Location = e.Location,
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            Capacity = e.Capacity,
            CategoryId = e.CategoryId,
            CategoryName = e.Category.Name
        };
    }


    // POST: api/events
    // 👇 requires any logged-in user
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<EventDto>> Create(CreateEventDto dto)
    {
        var category = await _context.Categories.FindAsync(dto.CategoryId);
        if (category == null)
            return BadRequest($"Category {dto.CategoryId} does not exist.");

        var e = new Event
        {
            Title = dto.Title,
            Description = dto.Description,
            Location = dto.Location,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Capacity = dto.Capacity,
            CategoryId = dto.CategoryId
        };

        await _repository.AddAsync(e);

        var result = new EventDto
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            Location = e.Location,
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            Capacity = e.Capacity,
            CategoryId = e.CategoryId,
            CategoryName = category.Name
        };

        return CreatedAtAction(nameof(GetById), new { id = e.Id }, result);
    }


    // PUT: api/events/5
    // 👇 requires any logged-in user
    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateEventDto dto)
    {
        var e = await _context.Events.FindAsync(id);
        if (e == null)
            return NotFound();

        var category = await _context.Categories.FindAsync(dto.CategoryId);
        if (category == null)
            return BadRequest($"Category {dto.CategoryId} does not exist.");

        e.Title = dto.Title;
        e.Description = dto.Description;
        e.Location = dto.Location;
        e.StartTime = dto.StartTime;
        e.EndTime = dto.EndTime;
        e.Capacity = dto.Capacity;
        e.CategoryId = dto.CategoryId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/events/5
    //  requires any logged-in user
    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _context.Events.FindAsync(id);
        if (e == null)
            return NotFound();

        _context.Events.Remove(e);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
