using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorShared.Models;

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

    public static async Task<EditCatalogItemResult> ToEditCatalogItemResultAsync(
    this Task<CatalogItemDTO> itemTask)
    {
        var item = await itemTask;

        var editCatalogItemResult = new EditCatalogItemResult
        {
            CatalogItem = new CatalogItem
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                PictureUri = item.PictureUri,
                CatalogTypeId = item.CatalogTypeId,
                CatalogBrandId = item.CatalogBrandId
            }
        };

        return editCatalogItemResult;
    }

    public static async Task<List<CatalogItem>> ToCatalogItemListAsync(
        this Task<ListPagedCatalogItemResponse> typeTask)
    {
        var types = await typeTask;

        var items = types.CatalogItems
            .Select(b => new CatalogItem
            {
                Id = b.Id,
                Name = b.Name,
                CatalogBrandId = b.CatalogBrandId,
                CatalogTypeId = b.CatalogTypeId,
                Description = b.Description,
                Price = b.Price,
                PictureUri = b.PictureUri
            })
            .ToList();

        return items;
    }
}
