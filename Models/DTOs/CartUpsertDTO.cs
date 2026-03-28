using System.ComponentModel.DataAnnotations;

namespace AspNetMvcProject.Models.DTOs;

public class CartUpsertDTO
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, 1000)]
    public int Count { get; set; }
}
