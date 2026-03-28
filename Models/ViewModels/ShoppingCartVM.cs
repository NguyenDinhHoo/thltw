using AspNetMvcProject.Models;

namespace AspNetMvcProject.Models.ViewModels;

public class ShoppingCartVM
{
    public IEnumerable<ShoppingCart> ShoppingCartList { get; set; }
    public OrderHeader OrderHeader { get; set; }
}
