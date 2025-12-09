using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Campus_events_api.Controllers;
using Campus_events_api.Data;
using Campus_events_api.Dtos;
using Campus_events_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Campus_events_api.Tests;

public class BookingTests
{
    private ApplicationDbContext CreateInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    private ClaimsPrincipal CreateFakeUser(int userId, string name = "Test User")
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.Name, name)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task CreateBooking_ShouldReturnBadRequest_WhenEventIsFull()
    {
        var db = CreateInMemoryDbContext(nameof(CreateBooking_ShouldReturnBadRequest_WhenEventIsFull));

        var evnt = new Event
        {
            Title = "Full Event",
            Description = "Already full",
            Location = "Room 1",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            Capacity = 1,
            CategoryId = 1
        };

        db.Events.Add(evnt);
        await db.SaveChangesAsync();

        db.Bookings.Add(new Booking
        {
            EventId = evnt.Id,
            UserId = 999,
            Status = "Active",
            BookedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var controller = new BookingsController(db);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreateFakeUser(1, "Student One")
            }
        };

        var dto = new CreateBookingDto { EventId = evnt.Id };

        var result = await controller.Create(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Event is fully booked.", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateBooking_ShouldSucceed_WhenCapacityAvailable()
    {
        var db = CreateInMemoryDbContext(nameof(CreateBooking_ShouldSucceed_WhenCapacityAvailable));

        var evnt = new Event
        {
            Title = "Available Event",
            Description = "Has space",
            Location = "Room 2",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            Capacity = 2,
            CategoryId = 1
        };

        db.Events.Add(evnt);
        await db.SaveChangesAsync();

        var controller = new BookingsController(db);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreateFakeUser(1, "Student One")
            }
        };

        var dto = new CreateBookingDto { EventId = evnt.Id };

        var result = await controller.Create(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var bookingDto = Assert.IsType<BookingDto>(createdResult.Value);
        Assert.Equal("Active", bookingDto.Status);
        Assert.Equal(evnt.Id, bookingDto.EventId);
        Assert.Equal(1, bookingDto.UserId);
    }
}
