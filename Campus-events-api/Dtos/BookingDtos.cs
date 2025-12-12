namespace Campus_events_api.Dtos;
using System.ComponentModel.DataAnnotations;

public class BookingDto
{
    public int Id { get; set; }
    public DateTime BookedAt { get; set; }
    public string Status { get; set; } = null!;

    public int EventId { get; set; }
    public string EventTitle { get; set; } = null!;
    public DateTime EventStartTime { get; set; }

    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
}

public class CreateBookingDto
{
    [Range(1, int.MaxValue, ErrorMessage = "EventId must be a positive number.")]
    public int EventId { get; set; }
}