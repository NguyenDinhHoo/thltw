using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AspNetMvcProject.Models.DTOs;

public class ProductCreateDTO
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0.01, 10000.0)]
    public decimal Price { get; set; }

    public int Stock { get; set; }

    public int? CategoryId { get; set; }

    public List<IFormFile>? Images { get; set; }
}
