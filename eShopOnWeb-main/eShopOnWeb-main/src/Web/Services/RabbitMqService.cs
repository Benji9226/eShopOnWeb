using System.Text;
using System.Text.Json;
using BlazorShared.Models;
using Microsoft.AspNetCore.Connections;
using Microsoft.eShopWeb.Web.Pages.Basket;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using static IRabbitMqService;

public class RabbitMqService : IRabbitMqService
{
    private readonly ConnectionFactory _factory;

    public RabbitMqService(RabbitMqOptions options)
    {
        _factory = new ConnectionFactory
        {
            HostName = options.HostName,
            UserName = options.UserName,
            Password = options.Password,
            Port = options.Port
        };
    }

    public async Task<ReserveResponse> ReserveItemAsync(int itemId, int amount)
    {
        return await ReserveAsync([new() { itemId = itemId, amount = amount }]);
    }

    public async Task<ReserveResponse> ReserveBasketAsync(IEnumerable<BasketItemViewModel> items)
    {
        var rpcItems = items.Select(i => new ReserveItem
        {
            itemId = i.CatalogItemId,
            amount = i.Quantity
        }).ToList();

        return await ReserveAsync(rpcItems);
    }

    private async Task<ReserveResponse> ReserveAsync(List<ReserveItem> items)
    {
        using var connection = await _factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();  

        var replyQueue = await channel.QueueDeclareAsync(queue: "", exclusive: true);
        var consumer = new AsyncEventingBasicConsumer(channel);

        var tcs = new TaskCompletionSource<ReserveResponse>();
        var correlationId = Guid.NewGuid().ToString();

        consumer.ReceivedAsync += (model, ea) =>
        {
            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                var response = JsonSerializer.Deserialize<ReserveResponse>(Encoding.UTF8.GetString(ea.Body.ToArray()));
                tcs.SetResult(response!);
            }
            return Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(replyQueue.QueueName, true, consumer);

        var messageBody = JsonSerializer.Serialize(items);
        var body = Encoding.UTF8.GetBytes(messageBody);

        var props = new BasicProperties();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueue.QueueName;

        await channel.BasicPublishAsync(
            exchange: "catalog_item_stock.exchange",
            routingKey: "catalog_item_stock.reserve",
            true,
            basicProperties: props,
            body: body
        );

        return await tcs.Task;
    }

    public class ReserveItem
    {
        public int itemId { get; set; }
        public int amount { get; set; }
    }
}
