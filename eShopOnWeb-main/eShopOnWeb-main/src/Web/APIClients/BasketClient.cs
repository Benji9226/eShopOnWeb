using System.Net;
using Ardalis.Result;
using Microsoft.eShopWeb.Web.DTOs;

namespace Microsoft.eShopWeb.Web.APIClients;

public interface IBasketClient
{
    Task<BasketDTO> GetOrCreateBasketByBuyerId(string userName);
    Task<int> CountTotalBasketItems(string username);
    Task<Result<BasketDTO>> SetQuantities(int basketId, Dictionary<string, int> quantities);

    Task<BasketDTO> AddItemToBasket(AddBasketItemDto addBasketItemDto);
}

public class BasketClient : IBasketClient
{
    private const string BaseUrl = "http://localhost:5004/api/basket/";
    private readonly HttpClient _httpClient;

    public BasketClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<BasketDTO> GetOrCreateBasketByBuyerId(string userName)
    {
        var result= _httpClient.GetFromJsonAsync<BasketDTO>(BaseUrl + userName);

        return result;
    }

    public Task<int> CountTotalBasketItems(string username)
    {
        var result = _httpClient.GetFromJsonAsync<int>($"{BaseUrl}count/{username}");

        return result;
    }

    public Task<Result<BasketDTO>> SetQuantities(int basketId, Dictionary<string, int> quantities)
    {
        var dto = new UpdateQuantitiesDto(basketId, quantities);

        var result = _httpClient.PutAsJsonAsync(BaseUrl + "setQuantities", dto);

        if (result.Result.StatusCode == HttpStatusCode.NotFound)
        {
            return Task.FromResult(Result<BasketDTO>.NotFound());
        }

        return result.Result.Content.ReadFromJsonAsync<Result<BasketDTO>>();

    }

    public Task<BasketDTO> AddItemToBasket(AddBasketItemDto addBasketItemDto)
    {
        var result = _httpClient.PostAsJsonAsync($"{BaseUrl}addItem", addBasketItemDto)
            .Result.Content.ReadFromJsonAsync<BasketDTO>();

        return result;
    }
    
    
    
}
