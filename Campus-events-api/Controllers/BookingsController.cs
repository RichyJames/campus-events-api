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

    // GET: api/bookings (current user's bookings)
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetMyBookings()
    {
        // user id comes from JWT "sub" claim
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null)
            return Unauthorized();

        var userId = int.Parse(userIdClaim);

        var bookings = await _context.Bookings
            .Include(b => b.Event)
            .Include(b => b.User)
            .Where(b => b.UserId == userId)
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
    // creates a booking for the logged-in user, checks capacity
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingDto dto)
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null)
            return Unauthorized();

        var userId = int.Parse(userIdClaim);

        var evnt = await _context.Events
            .Include(e => e.Bookings)
            .FirstOrDefaultAsync(e => e.Id == dto.EventId);

        if (evnt == null)
            return BadRequest("Event does not exist.");

        // capacity check: count active bookings
        var activeBookings = evnt.Bookings.Count(b => b.Status == "Active");
        if (activeBookings >= evnt.Capacity)
            return BadRequest("Event is fully booked.");

        var booking = new Booking
        {
            EventId = evnt.Id,
            UserId = userId,
            Status = "Active",
            BookedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        // map to DTO
        var dtoResult = new BookingDto
        {
            Id = booking.Id,
            BookedAt = booking.BookedAt,
            Status = booking.Status,
            EventId = evnt.Id,
            EventTitle = evnt.Title,
            EventStartTime = evnt.StartTime,
            UserId = userId,
            UserName = User.Identity?.Name ?? ""
        };

        return CreatedAtAction(nameof(GetMyBookings), new { }, dtoResult);
    }

    // PUT: api/bookings/{id}/cancel
    [Authorize]
    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null)
            return Unauthorized();

        var userId = int.Parse(userIdClaim);

        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

        if (booking == null)
            return NotFound();

        if (booking.Status == "Cancelled")
            return BadRequest("Booking already cancelled.");

        booking.Status = "Cancelled";
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/bookings/{id}  (admin could hard-delete, optional)
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
}
