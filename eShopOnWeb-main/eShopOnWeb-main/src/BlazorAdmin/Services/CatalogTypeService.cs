using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorAdmin.Extensions;
using BlazorAdmin.Interfaces;
using BlazorAdmin.Models;
using BlazorShared.Models;

namespace BlazorAdmin.Services;

public class CatalogTypeService : ICatalogTypeService
{
    private readonly HttpService _httpService;
    public CatalogTypeService(HttpService httpService)
    {
        _httpService = httpService;
    }
    public async Task<List<CatalogType>> List()
    {
        return await _httpService.HttpGet<List<CatalogTypeDTO>>("types").ToCatalogTypeListAsync();
    }
}
