using System.ComponentModel.DataAnnotations;

namespace AspNetMvcProject.Models;

public class Product
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0.01, 10000.0)]
    public decimal Price { get; set; }

    public int Stock { get; set; }

    public int? CategoryId { get; set; }
    public virtual Category? Category { get; set; }

    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}
