using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Contracts.Orders;

namespace Microsoft.eShopWeb.Infrastructure.Clients.Orders;

public class OrderServiceClient : IOrderServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public OrderServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;

        // Ensure camelCase serialization and respect for JsonPropertyName attributes
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task CreateOrderAsync(CreateOrderDto order)
    {
        // Serialize manually to ensure nested Shipping object is preserved
        var json = JsonSerializer.Serialize(order, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/v1/orders/", content);

        var body = await response.Content.ReadAsStringAsync();
        Console.WriteLine(body);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"StatusCode: {response.StatusCode}");
        }

    }

    public async Task<OrderReadDto> GetOrderByIdAsync(int orderId) =>
        await _httpClient.GetFromJsonAsync<OrderReadDto>($"/api/v1/orders/{orderId}", _jsonOptions);

    public async Task<List<OrderReadDto>> GetOrdersForUserAsync(string buyerId) =>
        await _httpClient.GetFromJsonAsync<List<OrderReadDto>>($"/api/v1/orders/?buyer_id={buyerId}", _jsonOptions);
}
