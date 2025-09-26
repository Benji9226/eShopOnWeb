using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorAdmin.Models;

public class CatalogTypeDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}
