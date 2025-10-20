using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Contracts.Orders;
using Microsoft.Extensions.Configuration;

namespace Microsoft.eShopWeb.Infrastructure.Clients.Orders;

public class OrderServiceClient : IOrderServiceClient
{
    private readonly HttpClient _httpClient;

    public OrderServiceClient(HttpClient httpClient, IConfiguration configuration)
    {
        var baseUrl = configuration["baseUrls:ordersMicroservice"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("Missing configuration for baseUrls:ordersMicroservice");
        }

        if (!baseUrl.EndsWith("/")) baseUrl += "/";

        httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient = httpClient;
    }

    public async Task CreateOrderAsync(CreateOrderDto order) =>
        await _httpClient.PostAsJsonAsync("api/v1/orders", order);

    public async Task<OrderReadDto> GetOrderByIdAsync(int orderId) =>
        await _httpClient.GetFromJsonAsync<OrderReadDto>($"api/v1/orders/{orderId}");

    public async Task<List<OrderReadDto>> GetOrdersForUserAsync(string buyerId) =>
        await _httpClient.GetFromJsonAsync<List<OrderReadDto>>($"api/v1/orders?buyer_id={buyerId}");
}
