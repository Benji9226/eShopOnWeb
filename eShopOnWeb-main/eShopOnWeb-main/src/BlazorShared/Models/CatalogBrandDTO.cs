using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorShared.Models;

public class CatalogBrandDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class ListCatalogBrandsResponse
{
    [JsonPropertyName("catalog_brands")]
    public List<CatalogBrandDto> CatalogBrands { get; set; } = new();
}
