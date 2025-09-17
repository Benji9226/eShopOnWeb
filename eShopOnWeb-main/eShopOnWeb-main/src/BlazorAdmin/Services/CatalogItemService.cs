using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorAdmin.Extensions;
using BlazorShared.Interfaces;
using BlazorShared.Models;
using Microsoft.Extensions.Logging;


namespace BlazorAdmin.Services;

public class CatalogItemService : ICatalogItemService
{
    private readonly ICatalogLookupDataService<CatalogBrand> _brandService;
    private readonly ICatalogLookupDataService<CatalogType> _typeService;
    private readonly HttpService _httpService;
    private readonly ILogger<CatalogItemService> _logger;

    public CatalogItemService(ICatalogLookupDataService<CatalogBrand> brandService,
        ICatalogLookupDataService<CatalogType> typeService,
        HttpService httpService,
        ILogger<CatalogItemService> logger)
    {
        _brandService = brandService;
        _typeService = typeService;
        _httpService = httpService;
        _logger = logger;
    }

    public async Task<CatalogItem> Create(CreateCatalogItemRequest catalogItem)
    {
        var response = await _httpService.HttpPost<CreateCatalogItemResponse>("catalog-items", catalogItem);
        return response?.CatalogItem;
    }

    public async Task<CatalogItem> Edit(CatalogItem catalogItem)
    {
        return (await _httpService.HttpPut<EditCatalogItemResult>("catalog-items", catalogItem)).CatalogItem;
    }

    public async Task<string> Delete(int catalogItemId)
    {
        return (await _httpService.HttpDelete<DeleteCatalogItemResponse>("catalog-items", catalogItemId)).Status;
    }

    public async Task<CatalogItem> GetById(int id)
    {
        var brandListTask = _brandService.List();
        var typeListTask = _typeService.List();
        var itemGetTask = _httpService.HttpGet<EditCatalogItemResult>($"catalog-items/{id}");
        await Task.WhenAll(brandListTask, typeListTask, itemGetTask);
        var brands = brandListTask.Result;
        var types = typeListTask.Result;
        var catalogItem = itemGetTask.Result.CatalogItem;
        catalogItem.CatalogBrand = brands.FirstOrDefault(b => b.Id == catalogItem.CatalogBrandId)?.Name;
        catalogItem.CatalogType = types.FirstOrDefault(t => t.Id == catalogItem.CatalogTypeId)?.Name;
        return catalogItem;
    }

    public async Task<List<CatalogItem>> ListPaged(int pageSize)
    {
        _logger.LogInformation("Fetching catalog items from API.");

        var brandListTask = _httpService.HttpGet<List<CatalogBrandDTO>>("brands").ToCatalogBrandListAsync();
        var typeListTask = _httpService.HttpGet<List<CatalogTypeDTO>>("types").ToCatalogTypeListAsync();
        var itemListTask = _httpService.HttpGet<ListPagedCatalogItemResponse>($"items?pageSize={pageSize}").ToCatalogItemListAsync();
        await Task.WhenAll(brandListTask, typeListTask, itemListTask);

        var brands = brandListTask.Result;
        var types = typeListTask.Result;
        var items = itemListTask.Result;

        foreach (var item in items)
        {
            item.CatalogBrand = brands.FirstOrDefault(b => b.Id == item.CatalogBrandId)?.Name;
            item.CatalogType = types.FirstOrDefault(t => t.Id == item.CatalogTypeId)?.Name;
        }
        return items;
    }

    public async Task<List<CatalogItem>> List()
    {
        _logger.LogInformation("Fetching catalog items from API.");

        var brandListTask = _httpService.HttpGet<List<CatalogBrandDTO>>("brands").ToCatalogBrandListAsync();
        var typeListTask = _httpService.HttpGet<List<CatalogTypeDTO>>("types").ToCatalogTypeListAsync();
        var itemListTask = _httpService.HttpGet<ListPagedCatalogItemResponse>($"items").ToCatalogItemListAsync();
        await Task.WhenAll(brandListTask, typeListTask, itemListTask);

        var brands = brandListTask.Result;
        var types = typeListTask.Result;
        var items = itemListTask.Result;

        foreach (var item in items)
        {
            item.CatalogBrand = brands.FirstOrDefault(b => b.Id == item.CatalogBrandId)?.Name;
            item.CatalogType = types.FirstOrDefault(t => t.Id == item.CatalogTypeId)?.Name;
        }
        return items;
    }
}
