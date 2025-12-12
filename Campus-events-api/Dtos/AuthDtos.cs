namespace Campus_events_api.Dtos;
using System.ComponentModel.DataAnnotations;
public class RegisterDto
{
    [Required]
    [StringLength(60, MinimumLength = 2)]
    public string Name { get; set; } = null!;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    [Required]
    [StringLength(50, MinimumLength = 6)]
    public string Password { get; set; } = null!;
    [Required]
    [RegularExpression("^(Student|Organiser|Admin)$",
        ErrorMessage = "Role must be Student, Organiser, or Admin.")]
    public string Role { get; set; } = "Student"; 
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string Token { get; set; } = null!;
}