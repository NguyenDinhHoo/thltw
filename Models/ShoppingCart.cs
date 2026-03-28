using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetMvcProject.Models;

public class ShoppingCart
{
    [Key]
    public int Id { get; set; }

    public int ProductId { get; set; }
    [ForeignKey("ProductId")]
    [ValidateNever]
    public Product Product { get; set; }

    [Range(1, 1000, ErrorMessage = "Vui lòng nhập số lượng từ 1 đến 1000")]
    public int Count { get; set; }

    public int ApplicationUserId { get; set; }
    [ForeignKey("ApplicationUserId")]
    [ValidateNever]
    public ApplicationUser ApplicationUser { get; set; }

    [NotMapped]
    public double Price { get; set; }
}
