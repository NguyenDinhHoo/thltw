using System.ComponentModel.DataAnnotations;

namespace AspNetMvcProject.Models.DTOs;

public class ProductDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public List<ProductImageDTO> Images { get; set; } = new List<ProductImageDTO>();
}

public class ProductImageDTO
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
