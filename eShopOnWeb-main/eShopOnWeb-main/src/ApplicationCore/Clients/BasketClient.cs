using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.DTOs;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;

namespace Microsoft.eShopWeb.ApplicationCore.Clients;

public interface IBasketClient
{
    Task<BasketDTO?> GetBasket(int basketId);
}

public class BasketClient : IBasketClient
{
    //todo config
    private const string BaseUrl = "http://localhost:5004/api/basket";
    private readonly HttpClient _httpClient;

    public BasketClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BasketDTO?> GetBasket(int basketId)
    {
        var response = await _httpClient.GetFromJsonAsync<BasketDTO>($"{BaseUrl}/{basketId}");
        
        return response;
    }
}
