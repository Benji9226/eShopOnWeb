using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.eShopWeb.Web.Pages.Basket;

public interface IRabbitMqService
{
    Task<ReserveResponse> ReserveItemAsync(int itemId, int amount);
    Task<ReserveResponse> ReserveBasketAsync(IEnumerable<BasketItemViewModel> items);

    public record ReserveResponse
    {
        public bool success { get; init; }
        public string? reason { get; init; }
    }
}
