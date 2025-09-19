using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorAdmin.Models;
using BlazorShared.Models;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;

namespace BlazorAdmin.Extensions;

public static class CatalogExtensions
{
    public static async Task<List<CatalogBrand>> ToCatalogBrandListAsync(
        this Task<List<CatalogBrandDTO>> brandTask)
    {
        var brands = await brandTask;

        var items = brands
            .Select(b => new CatalogBrand
            {
                Id = b.Id,
                Name = b.Brand
            })
            .ToList();

        return items;
    }

    public static async Task<List<CatalogType>> ToCatalogTypeListAsync(
        this Task<List<CatalogTypeDTO>> typeTask)
    {
        var types = await typeTask;

        var items = types
            .Select(b => new CatalogType
            {
                Id = b.Id,
                Name = b.Type
            })
            .ToList();

        return items;
    }

    public static async Task<CatalogItem> ToCatalogItemAsync(
    this Task<CatalogItemDTO> itemTask, IUriComposer uriComposer)
    {
        var item = await itemTask;

        var catalogItem = new CatalogItem
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            PictureUri = uriComposer.ComposePicUri(item.PictureUri),
            CatalogTypeId = item.CatalogTypeId,
            CatalogBrandId = item.CatalogBrandId
        };

        return catalogItem;
    }

    public static async Task<List<CatalogItem>> ToCatalogItemListAsync(
        this Task<ListPagedCatalogItemResponse> typeTask, IUriComposer uriComposer)
    {
        var types = await typeTask;

        var items = types.CatalogItems
            .Select(item => new CatalogItem
            {
                Id = item.Id,
                Name = item.Name,
                CatalogBrandId = item.CatalogBrandId,
                CatalogTypeId = item.CatalogTypeId,
                Description = item.Description,
                Price = item.Price,
                PictureUri = uriComposer.ComposePicUri(item.PictureUri),
            })
            .ToList();

        return items;
    }
}
