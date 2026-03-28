using AspNetMvcProject.Models;

namespace AspNetMvcProject.Models.ViewModels;

public class OrderVM
{
    public OrderHeader OrderHeader { get; set; }
    public IEnumerable<OrderDetail> OrderDetails { get; set; }
}
