using Microsoft.eShopWeb.Web.Pages.Basket;

namespace Microsoft.eShopWeb.Web.DTOs;

public class BasketItemDTO
{
    public int Id { get; set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public int CatalogItemId { get; private set; }
    
    public BasketItemViewModel ToViewModel()
    {
        return new BasketItemViewModel
        {
            Id = this.Id,
            UnitPrice = this.UnitPrice,
            Quantity = this.Quantity,
            CatalogItemId = this.CatalogItemId,
        };
    }
}
