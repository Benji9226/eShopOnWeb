﻿using System.Text.Json.Serialization;

namespace Microsoft.eShopWeb.Web.DTOs;

public class CatalogTypeDTO
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}
