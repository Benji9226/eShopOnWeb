using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorAdmin.Models;

public class CatalogBrandDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("brand")]
    public string Brand { get; set; } = string.Empty;
}
