using System.Text;
using System.Text.Json;
using BlazorShared.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.eShopWeb.Web.Cache;
using Microsoft.eShopWeb.Web.Hubs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using static IRabbitMqService;

namespace Microsoft.eShopWeb.Web.Subscribers;

public class StockSubscriber
{
    private readonly IHubContext<StockHub> _hub;
    private readonly Dictionary<int, StockItem> _cache = new();
    private readonly ConnectionFactory _factory;

    public StockSubscriber(IHubContext<StockHub> hub, RabbitMqOptions options)
    {
        _hub = hub;

        _factory = new ConnectionFactory
        {
            HostName = options.HostName,
            UserName = options.UserName,
            Password = options.Password,
            Port = options.Port
        };

        // fire-and-forget listener safely
        _ = StartListening();
    }

    private async Task StartListening()
    {
        var connection = await _factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        // Declare a temporary queue for all stock events
        var queue = await channel.QueueDeclareAsync("", exclusive: true);
        await channel.QueueBindAsync(queue.QueueName, "catalog_item_stock.exchange", "catalog_item_stock.*");

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            try
            {
                var routingKey = ea.RoutingKey;
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var items = JsonSerializer.Deserialize<List<Item>>(json, options);

                if (items != null)
                {
                    foreach (var item in items)
                        await HandleMessageAsync(item, routingKey);
                }
            }
            catch (Exception ex)
            {
                // optionally log exceptions here
                Console.WriteLine($"Error handling RabbitMQ message: {ex}");
            }
        };

        await channel.BasicConsumeAsync(queue.QueueName, autoAck: true, consumer: consumer);
    }

    private async Task HandleMessageAsync(Item msg, string routingKey)
    {
        if (!_cache.ContainsKey(msg.itemId))
        {
            _cache[msg.itemId] = new StockItem
            {
                ItemId = msg.itemId,
                Total = 0,
                Reserved = 0
            };
        }

        var stock = _cache[msg.itemId];

        switch (routingKey)
        {
            case "catalog_item_stock.restock.success":
                stock.Total += msg.amount;
                break;
            case "catalog_item_stock.reserve.success":
                stock.Reserved += msg.amount;
                break;
            case "catalog_item_stock.cancel.success":
                stock.Reserved -= msg.amount;
                break;
            case "catalog_item_stock.confirm.success":
                stock.Reserved -= msg.amount;
                stock.Total -= msg.amount;
                break;
            default:
                // unknown routing key
                return;
        }

        // broadcast update to all connected SignalR clients
        await _hub.Clients.All.SendAsync("StockUpdated", stock);
    }

    public void UpdateCache(StockItem item)
    {
        _cache[item.ItemId] = item;
    }

    // optional: allow querying current cache
    public IReadOnlyDictionary<int, StockItem> GetCache() => _cache;
}
