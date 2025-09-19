using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Contracts.Orders;
public class OrderReadDto
{
    public int Id { get; set; }
    public string BuyerId { get; set; }
    public DateTime OrderDate { get; set; }
    public List<OrderItemDto> Items { get; set; }
    public ShippingAddressDto Shipping { get; set; }

    public decimal Total { get; set; }

}
