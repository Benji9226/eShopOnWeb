using Microsoft.eShopWeb.Web.Pages.Basket;

namespace Microsoft.eShopWeb.Web.DTOs;

public class BasketDTO
{
    public int Id { get; set; }
    public string BuyerId { get; set; }
    public List<BasketItemDTO> Items { get; set; }
    
    public BasketViewModel ToViewModel()
    {
        return new BasketViewModel
        {
            Id = this.Id,
            BuyerId = this.BuyerId,
            Items = this.Items?.Select(i => i.ToViewModel()).ToList() ?? new List<BasketItemViewModel>()
        };
    }
}
