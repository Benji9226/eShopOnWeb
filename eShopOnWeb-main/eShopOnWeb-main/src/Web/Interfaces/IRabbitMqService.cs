﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.eShopWeb.Web.Pages.Basket;

public interface IRabbitMqService
{
    Task<ReserveResponse> ReserveItemAsync(int itemId, int amount);
    Task<ReserveResponse> ReserveAsync(List<Item> items);
    Task SendConfirmAsync(List<Item> items);
    Task SendCancelAsync(List<Item> items);
    Task SendRestockAsync(List<Item> items);

    public class ReserveResponse
    {
        public bool success { get; set; }
        public string? reason { get; set; }
    }

    public class Item
    {
        public int itemId { get; set; }
        public int amount { get; set; }
    }
}
