using Microsoft.AspNetCore.SignalR;
using Microsoft.eShopWeb.Web.Cache;
using Microsoft.eShopWeb.Web.Subscribers;
using static IRabbitMqService;

namespace Microsoft.eShopWeb.Web.Hubs;

public class StockHub : Hub
{
    private readonly RabbitMqService _rabbitMqService;
    private readonly StockSubscriber _subscriber;

    public StockHub(RabbitMqService rabbitMqService, StockSubscriber subscriber)
    {
        _rabbitMqService = rabbitMqService;
        _subscriber = subscriber;
    }

    public async Task Restock(int itemId, int amount)
    {
        await _rabbitMqService.SendRestockAsync(new List<Item> { new() { itemId=itemId, amount=amount } });
    }

    public async Task<List<StockItem>> GetStockAsync()
    {
        var items = await _rabbitMqService.GetFullStockAsync();

        // Update the in-memory cache
        foreach (var item in items)
            _subscriber.UpdateCache(item);

        return items;
    }
}
