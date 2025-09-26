using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorAdmin.Interfaces;
using BlazorAdmin.Models;
using Blazored.LocalStorage;
using BlazorShared.Models;
using Microsoft.Extensions.Logging;

namespace BlazorAdmin.Services;

public class CachedCatalogBrandServiceDecorator : ICatalogBrandService
{
    private readonly ILocalStorageService _localStorageService;
    private readonly CatalogBrandService _catalogBrandService;
    private ILogger<CachedCatalogBrandServiceDecorator> _logger;

    public CachedCatalogBrandServiceDecorator(ILocalStorageService localStorageService,
        CatalogBrandService catalogBrandService,
        ILogger<CachedCatalogBrandServiceDecorator> logger)
    {
        _localStorageService = localStorageService;
        _catalogBrandService = catalogBrandService;
        _logger = logger;
    }

    public async Task<List<CatalogBrand>> List()
    {
        string key = typeof(CatalogBrand).Name;
        var cacheEntry = await _localStorageService.GetItemAsync<CacheEntry<List<CatalogBrand>>>(key);
        if (cacheEntry != null)
        {
            _logger.LogInformation($"Loading {key} from local storage.");
            if (cacheEntry.DateCreated.AddMinutes(1) > DateTime.UtcNow)
            {
                return cacheEntry.Value;
            }
            else
            {
                _logger.LogInformation($"Cache expired; removing {key} from local storage.");
                await _localStorageService.RemoveItemAsync(key);
            }
        }

        var types = await _catalogBrandService.List();
        var entry = new CacheEntry<List<CatalogBrand>>(types);
        await _localStorageService.SetItemAsync(key, entry);
        return types;
    }
}
