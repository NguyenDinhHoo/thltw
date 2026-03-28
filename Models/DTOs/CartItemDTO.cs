namespace AspNetMvcProject.Models.DTOs;

public class CartItemDTO
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public decimal ProductPrice { get; set; }
    public int Count { get; set; }
    public decimal TotalPrice => ProductPrice * Count;
}
