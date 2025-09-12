using System.Text.Json;
using Microsoft.eShopWeb.Web.DTOs;
using Microsoft.eShopWeb.Web.ViewModels;
using static System.Net.WebRequestMethods;

namespace Microsoft.eShopWeb.Web.Clients;

public interface ICatalogApiClient
{
    Task<ListPagedCatalogItemResponse> GetCatalogItemsAsync(int pageIndex, int pageSize, int? brandId = null, int? typeId = null);
    Task<List<CatalogBrandDto>> GetBrandsAsync();
    Task<ListCatalogTypesResponse> GetCatalogTypesAsync();
    Task<CatalogItemDTO> GetCatalogItemAsync(int catalogItemId);
    Task<CatalogItemDTO> UpdateCatalogItemAsync(CatalogItemDTO item);
}

public class CatalogApiClient : ICatalogApiClient
{
    private readonly HttpClient _httpClient;

    public CatalogApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ListCatalogTypesResponse> GetCatalogTypesAsync()
    {
        var response = await _httpClient.GetAsync("types");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ListCatalogTypesResponse>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new ListCatalogTypesResponse();
    }

    public async Task<List<CatalogBrandDto>> GetBrandsAsync()
    {
        var response = await _httpClient.GetAsync("brands");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ListCatalogBrandsResponse>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result?.CatalogBrands ?? new List<CatalogBrandDto>();
    }

    public async Task<ListPagedCatalogItemResponse> GetCatalogItemsAsync(
        int pageIndex, int pageSize, int? brandId = null, int? typeId = null)
    {
        var url = $"catalog-items?pageSize={pageSize}&pageIndex={pageIndex}";

        if (brandId.HasValue) url += $"&catalogBrandId={brandId.Value}";
        if (typeId.HasValue) url += $"&catalogTypeId={typeId.Value}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ListPagedCatalogItemResponse>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new ListPagedCatalogItemResponse();
    }

    public async Task<CatalogItemDTO> GetCatalogItemAsync(int catalogItemId)
    {
        var url = $"catalog-items/{catalogItemId}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CatalogItemDTO>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new CatalogItemDTO();
    }

    public async Task<CatalogItemDTO> UpdateCatalogItemAsync(CatalogItemDTO item)
    {
        var response = await _httpClient.PutAsJsonAsync("/catalog-items", item);

        response.EnsureSuccessStatusCode();

        var updated = await response.Content.ReadFromJsonAsync<CatalogItemDTO>();
        return updated!;
    }
}
