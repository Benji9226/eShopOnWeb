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
    Task TransferBasketAsync(string anonymousId, string userName);
    Task DeleteBasketAsync(int basketId);
}

public class BasketClient : IBasketClient
{
    
    //todo config
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

    public async Task<Result<BasketDTO>> SetQuantities(int basketId, Dictionary<string, int> quantities)
    {
        var dto = new UpdateQuantitiesDto(basketId, quantities);

        var response = await _httpClient.PatchAsJsonAsync(BaseUrl + "setQuantities", dto);

        var result = await response.Content.ReadFromJsonAsync<Result<BasketDTO>>();
        return result;
    }

    public Task<BasketDTO> AddItemToBasket(AddBasketItemDto addBasketItemDto)
    {
        var result = _httpClient.PostAsJsonAsync($"{BaseUrl}addItem", addBasketItemDto)
            .Result.Content.ReadFromJsonAsync<BasketDTO>();

        return result;
    }
    
    public async Task TransferBasketAsync(string anonymousId, string userName)
    {
        var dto = new TransferDTO(anonymousId, userName);

        var response = await _httpClient.PutAsJsonAsync(BaseUrl + "transfer", dto);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Error transferring basket");
        }
    }
    
    public async Task DeleteBasketAsync(int basketId)
    {
        var response = await _httpClient.DeleteAsync(BaseUrl + basketId);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Error deleting basket");
        }
    }
    
}
