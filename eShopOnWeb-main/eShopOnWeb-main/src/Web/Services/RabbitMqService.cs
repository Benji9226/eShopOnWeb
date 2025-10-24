﻿using System.Text;
using System.Text.Json;
using BlazorShared.Models;
using Microsoft.eShopWeb.Web.Cache;
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

    #region Reserve

    public async Task<ReserveResponse> ReserveItemAsync(int itemId, int amount)
    {
        return await ReserveAsync(new List<Item> { new() { itemId = itemId, amount = amount } });
    }

    public async Task<ReserveResponse> ReserveAsync(List<Item> items)
    {
        await using var connection = await _factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        var replyQueue = await channel.QueueDeclareAsync(queue: "", exclusive: true);

        var correlationId = Guid.NewGuid().ToString();
        var tcs = new TaskCompletionSource<ReserveResponse>(TaskCreationOptions.RunContinuationsAsynchronously);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var response = JsonSerializer.Deserialize<ReserveResponse>(json);
                tcs.TrySetResult(response!);
            }
            await Task.Yield();
        };

        await channel.BasicConsumeAsync(
            queue: replyQueue.QueueName,
            autoAck: true,
            consumer: consumer
        );

        var messageBody = JsonSerializer.Serialize(items);
        var body = Encoding.UTF8.GetBytes(messageBody);

        var props = new BasicProperties
        {
            CorrelationId = correlationId,
            ReplyTo = replyQueue.QueueName
        };

        await channel.BasicPublishAsync(
            exchange: "catalog_item_stock.exchange",
            routingKey: "catalog_item_stock.reserve",
            mandatory: true,
            basicProperties: props,
            body: body
        );

        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        if (completedTask != tcs.Task)
            throw new TimeoutException("RPC request timed out waiting for response");

        return await tcs.Task;
    }

    #endregion

    public async Task<List<StockItem>> GetFullStockAsync()
    {
        await using var connection = await _factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        var replyQueue = await channel.QueueDeclareAsync("", exclusive: true);
        var correlationId = Guid.NewGuid().ToString();
        var tcs = new TaskCompletionSource<List<StockItem>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, ea) =>
        {
            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var stock = JsonSerializer.Deserialize<List<StockItem>>(json);
                tcs.TrySetResult(stock!);
            }
            await Task.Yield();
        };

        await channel.BasicConsumeAsync(replyQueue.QueueName, autoAck: true, consumer: consumer);

        var props = new BasicProperties
        {
            CorrelationId = correlationId,
            ReplyTo = replyQueue.QueueName,
            Persistent = true
        };

        // Send empty body to request full stock
        var body = Encoding.UTF8.GetBytes("");
        await channel.BasicPublishAsync(
            exchange: "catalog_item_stock.exchange",
            routingKey: "catalog_item_stock.getall",
            mandatory: true,
            basicProperties: props,
            body: body
        );

        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(5000));
        if (completedTask != tcs.Task)
            throw new TimeoutException("Timeout waiting for full stock response");

        return await tcs.Task;
    }

    public async Task SendConfirmAsync(List<Item> items)
    {
        await PublishAsync("catalog_item_stock.confirm", items);
    }

    public async Task SendRestockAsync(List<Item> items)
    {
        await PublishAsync("catalog_item_stock.restock", items);
    }

    public async Task SendCancelAsync(List<Item> items)
    {
        await PublishAsync("catalog_item_stock.cancel", items);
    }

    private async Task PublishAsync(string routingKey, List<Item> items)
    {
        await using var connection = await _factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(items));

        var props = new BasicProperties
        {
            Persistent = true
        };

        await channel.BasicPublishAsync(
            exchange: "catalog_item_stock.exchange",
            routingKey: routingKey,
            true,
            basicProperties: props,
            body: body
        );
    }
}
