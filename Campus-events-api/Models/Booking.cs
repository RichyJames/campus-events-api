namespace Campus_events_api.Models;

public class Booking
{
    public int Id { get; set; }

    public DateTime BookedAt { get; set; } = DateTime.UtcNow;

    
    public string Status { get; set; } = "Active";


    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}