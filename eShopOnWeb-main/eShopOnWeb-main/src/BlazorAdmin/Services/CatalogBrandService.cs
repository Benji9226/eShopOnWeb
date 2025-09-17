using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorAdmin.Extensions;
using BlazorShared.Models;

namespace BlazorAdmin.Services;

public interface ICatalogBrandService
{
    Task<List<CatalogBrand>> List();
}
public class CatalogBrandService : ICatalogBrandService
{
    private readonly HttpService _httpService;
    public CatalogBrandService(HttpService httpService)
    {
        _httpService = httpService;
    }

    public async Task<List<CatalogBrand>> List()
    {
        return await _httpService.HttpGet<List<CatalogBrandDTO>>("brands").ToCatalogBrandListAsync();
    }
}
