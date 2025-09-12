using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorShared.Models;
using static System.Net.WebRequestMethods;

namespace BlazorAdmin.Services;

public interface ICatalogApiClient
{
    Task<ListPagedCatalogItemResponse> GetCatalogItemsAsync(int pageIndex, int pageSize, int? brandId = null, int? typeId = null);
    Task<List<CatalogBrandDto>> GetBrandsAsync();
    Task<List<CatalogTypeDTO>> GetCatalogTypesAsync();
    Task<CatalogItemDTO> GetCatalogItemAsync(int catalogItemId);
    Task<CatalogItemDTO> UpdateCatalogItemAsync(CatalogItemDTO item);
    Task<CatalogItemDTO> CreateCatalogItemAsync(CatalogItemDTO item);
    Task DeleteCatalogItemAsync(int catalogItemId);
}

public class CatalogApiClientService : ICatalogApiClient
{
    private readonly HttpClient _httpClient;

    public CatalogApiClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CatalogItemDTO> CreateCatalogItemAsync(CatalogItemDTO item)
    {
        var response = await _httpClient.PostAsJsonAsync("catalog-items", item);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<CatalogItemDTO>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return created!;
    }

    public async Task DeleteCatalogItemAsync(int catalogItemId)
    {
        var response = await _httpClient.DeleteAsync($"catalog-items/{catalogItemId}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException($"Catalog item {catalogItemId} not found.");
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task<List<CatalogTypeDTO>> GetCatalogTypesAsync()
    {
        var response = await _httpClient.GetAsync("types");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ListCatalogTypesResponse>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new ListCatalogTypesResponse();

        return result?.CatalogTypes ?? new List<CatalogTypeDTO>();
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
