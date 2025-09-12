using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorShared.Models;

public class CatalogItemDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
    [JsonPropertyName("picture_uri")]
    public string PictureUri { get; set; } = "";
    [JsonPropertyName("catalog_type_id")]
    public int CatalogTypeId { get; set; }
    [JsonPropertyName("catalog_brand_id")]
    public int CatalogBrandId { get; set; }


    public static CatalogItemDTO DTOToViewModel(CatalogItemDTO catalogItemViewModel) {         
        return new CatalogItemDTO
        {
            Id = catalogItemViewModel.Id,
            Name = catalogItemViewModel.Name,
            Price = catalogItemViewModel.Price,
            PictureUri = catalogItemViewModel.PictureUri
        };
    }
}

public class ListPagedCatalogItemResponse
{
    [JsonPropertyName("catalog_items")]
    public List<CatalogItemDTO> CatalogItems { get; set; } = new();
    [JsonPropertyName("page_count")]
    public int PageCount { get; set; }
}


