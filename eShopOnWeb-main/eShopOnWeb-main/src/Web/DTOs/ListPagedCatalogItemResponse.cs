using System.Text.Json.Serialization;

namespace Microsoft.eShopWeb.Web.DTOs;

public class ListPagedCatalogItemResponse
{
    [JsonPropertyName("catalog_items")]
    public List<CatalogItemDTO> CatalogItems { get; set; } = new();
    [JsonPropertyName("page_count")]
    public int PageCount { get; set; }
}
