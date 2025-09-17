using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorShared.Models;

public class    CatalogBrandDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("brand")]
    public string Brand { get; set; } = string.Empty;
}

public class ListCatalogBrandsResponse
{
    [JsonPropertyName("catalog_brands")]
    public List<CatalogBrandDTO> CatalogBrands { get; set; } = new();
}
