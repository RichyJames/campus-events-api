using System.ComponentModel.DataAnnotations;

namespace Campus_events_api.Models;

public class Event
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Location { get; set; } = null!;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public int Capacity { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}