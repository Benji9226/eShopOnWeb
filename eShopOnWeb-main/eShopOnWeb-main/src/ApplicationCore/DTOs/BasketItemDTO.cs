﻿namespace Microsoft.eShopWeb.ApplicationCore.DTOs;

public class BasketItemDTO
{
    public int Id { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public int CatalogItemId { get; set; }
}
