namespace Campus_events_api.Models;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    // hashed password, never store plain text
    public string PasswordHash { get; set; } = null!;

    // "Student", "Organiser", "Admin"
    public string Role { get; set; } = "Student";
}