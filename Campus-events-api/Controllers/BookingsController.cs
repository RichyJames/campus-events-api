using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Campus_events_api.Data;
using Campus_events_api.Dtos;
using Campus_events_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Campus_events_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BookingsController(ApplicationDbContext context)
    {
        _context = context;
    }

   
    private int? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(sub))
        {
            sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        if (string.IsNullOrWhiteSpace(sub))
        {
            return null;
        }

        if (int.TryParse(sub, out var id))
        {
            return id;
        }

        return null;
    }

    // GET: api/bookings  
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetMyBookings(
        [FromQuery] string? sortBy = "bookedAt",
        [FromQuery] string? order = "desc")
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        IQueryable<Booking> query = _context.Bookings
            .Include(b => b.Event)
            .Include(b => b.User)
            .Where(b =>
                b.UserId == userId.Value &&
                b.Status == "Active"
            );

        var sort = (sortBy ?? "bookedAt").Trim().ToLowerInvariant();
        var desc = (order ?? "desc").Trim().ToLowerInvariant() == "desc";

        query = sort switch
        {
            "status" => desc ? query.OrderByDescending(b => b.Status) : query.OrderBy(b => b.Status),
            "eventtitle" => desc ? query.OrderByDescending(b => b.Event.Title) : query.OrderBy(b => b.Event.Title),
            "eventstarttime" => desc ? query.OrderByDescending(b => b.Event.StartTime) : query.OrderBy(b => b.Event.StartTime),
            _ => desc ? query.OrderByDescending(b => b.BookedAt) : query.OrderBy(b => b.BookedAt),
        };

        var bookings = await query
            .Select(b => new BookingDto
            {
                Id = b.Id,
                BookedAt = b.BookedAt,
                Status = b.Status,
                EventId = b.EventId,
                EventTitle = b.Event.Title,
                EventStartTime = b.Event.StartTime,
                UserId = b.UserId,
                UserName = b.User.Name
            })
            .ToListAsync();

        return Ok(bookings);
    }

    // POST: api/bookings 
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        // load event with its existing bookings
        var evnt = await _context.Events
            .Include(e => e.Bookings)
            .FirstOrDefaultAsync(e => e.Id == dto.EventId);

        if (evnt == null)
            return BadRequest("Event does not exist.");

        var alreadyBooked = evnt.Bookings.Any(b =>
            b.UserId == userId.Value && b.Status == "Active");

        if (alreadyBooked)
            return BadRequest("You already have an active booking for this event.");

        // capacity check: count active bookings
        var activeBookings = evnt.Bookings.Count(b => b.Status == "Active");
        if (activeBookings >= evnt.Capacity)
            return BadRequest("Event is fully booked.");

        var booking = new Booking
        {
            EventId = evnt.Id,
            UserId = userId.Value,
            Status = "Active",
            BookedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        // map to DTO for response
        var dtoResult = new BookingDto
        {
            Id = booking.Id,
            BookedAt = booking.BookedAt,
            Status = booking.Status,
            EventId = evnt.Id,
            EventTitle = evnt.Title,
            EventStartTime = evnt.StartTime,
            UserId = userId.Value,
            UserName = User.Identity?.Name ?? ""
        };

        return CreatedAtAction(nameof(GetMyBookings), new { }, dtoResult);
    }

    // PUT: api/bookings/{id}/cancel 
    [Authorize]
    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var role =
            User.FindFirstValue(ClaimTypes.Role)
            ?? User.FindFirstValue("role")
            ?? "";

        Booking? booking;

        if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
            role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
        {
            booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
        }
        else
        {
            booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId.Value);
        }

        if (booking == null) return NotFound();

        if (booking.Status == "Cancelled")
            return BadRequest("Booking is already cancelled.");

        booking.Status = "Cancelled";
        await _context.SaveChangesAsync();

        return NoContent();
    }


    // DELETE: api/bookings/{id} 
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
            return NotFound();

        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    
// Organisers/Admins can see who booked a specific event
    [Authorize(Roles = "Organiser,Admin")]
    [HttpGet("event/{eventId:int}")]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookingsForEvent(int eventId)
    {
        var bookings = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Event)
            .Where(b => b.EventId == eventId && b.Status == "Active")
            .Select(b => new BookingDto
            {
                Id = b.Id,
                BookedAt = b.BookedAt,
                Status = b.Status,
                EventId = b.EventId,
                EventTitle = b.Event.Title,
                EventStartTime = b.Event.StartTime,
                UserId = b.UserId,
                UserName = b.User.Name
            })
            .ToListAsync();

        return Ok(bookings);
    }

}
