using System.ComponentModel.DataAnnotations;

namespace AspNetMvcProject.Models.DTOs;

public class OrderCreateDTO
{
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;
    [Required]
    public string StreetAddress { get; set; } = string.Empty;
    [Required]
    public string City { get; set; } = string.Empty;
    [Required]
    public string FullName { get; set; } = string.Empty;
}

public class OrderDTO
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public double OrderTotal { get; set; }
    public string? OrderStatus { get; set; }
    public string FullName { get; set; } = string.Empty;
}
