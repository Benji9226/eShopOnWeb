using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Contracts.Orders;
public class CreateOrderDto
{
    public string BuyerId { get; set; }
    public string ShipToAddress_Street { get; set; }
    public string ShipToAddress_City { get; set; }
    public string ShipToAddress_State { get; set; }
    public string ShipToAddress_Country { get; set; }
    public string ShipToAddress_ZipCode { get; set; }
    public List<OrderItemDto> Items { get; set; }
}
