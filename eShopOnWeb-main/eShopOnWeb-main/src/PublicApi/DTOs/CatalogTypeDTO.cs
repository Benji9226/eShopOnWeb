using System.Text.Json.Serialization;

namespace Microsoft.eShopWeb.PublicAPI.DTOs;

public class CatalogTypeDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class ListCatalogTypesResponse
{
    [JsonPropertyName("catalog_types")]
    public List<CatalogTypeDTO> CatalogTypes { get; set; } = new();
}
