namespace Campus_events_api.Dtos;
using System.ComponentModel.DataAnnotations;
public class EventDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Location { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Capacity { get; set; }

    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
}

public class CreateEventDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Title { get; set; } = null!;
    [StringLength(500)]
    public string Description { get; set; } = null!;
    [Required]
    [StringLength(120)]
    public string Location { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    [Range(1, 500, ErrorMessage = "Capacity must be between 1 and 500.")]
    public int Capacity { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be a positive number.")]
    public int CategoryId { get; set; }
}