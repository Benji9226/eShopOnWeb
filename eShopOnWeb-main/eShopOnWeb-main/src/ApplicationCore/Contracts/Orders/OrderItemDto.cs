using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Contracts.Orders;
public class OrderItemDto
{
    public int ItemOrdered_CatalogItemId { get; set; }
    public string ItemOrdered_ProductName { get; set; }
    public string ItemOrdered_PictureUri { get; set; }
    public decimal UnitPrice { get; set; }
    public int Units { get; set; }
}
