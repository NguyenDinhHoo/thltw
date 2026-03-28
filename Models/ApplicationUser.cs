using System.ComponentModel.DataAnnotations;

namespace AspNetMvcProject.Models;

public class ApplicationUser
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? FullName { get; set; }
    public string? Address { get; set; }
    public int? Age { get; set; }

    public string? Role { get; set; }
}
