using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorAdmin.Helpers;
using BlazorAdmin.Interfaces;
using BlazorAdmin.Models;
using BlazorAdmin.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlazorAdmin.Pages.CatalogItemPage;

public partial class List : BlazorComponent
{
    [Inject] public ICatalogItemService CatalogItemService { get; set; }
    [Inject] public ICatalogBrandService CatalogBrandService { get; set; }
    [Inject] public ICatalogTypeService CatalogTypeService { get; set; }
    [Inject] public NavigationManager NavigationManager { get; set; }
    [Inject] public StockHubService StockHub { get; set; }

    private List<CatalogItem> catalogItems = new();
    private List<CatalogType> catalogTypes = new();
    private List<CatalogBrand> catalogBrands = new();

    private Edit EditComponent { get; set; }
    private Delete DeleteComponent { get; set; }
    private Details DetailsComponent { get; set; }
    private Create CreateComponent { get; set; }

    private Dictionary<int, int> restockAmounts = new();

    protected override async Task OnInitializedAsync()
    {
        StockHub.OnStockUpdated(item => Console.WriteLine($"Updated {item.ItemId}: {item.Total}"));
        await StockHub.StartAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Load catalog and related data
            catalogItems = await CatalogItemService.List();
            catalogTypes = await CatalogTypeService.List();
            catalogBrands = await CatalogBrandService.List();

            foreach (var item in catalogItems)
                restockAmounts[item.Id] = 1;

            //StockHub.OnStockUpdated(UpdateStock);
            //await StockHub.StartAsync();

            //// Setup event listeners
            //hubConnection.On<StockItem>("StockUpdated", item =>
            //{
            //    var existing = catalogItems.Find(ci => ci.Id == item.ItemId);
            //    if (existing != null)
            //    {
            //        existing.Total = item.Total;
            //        existing.Reserved = item.Reserved;
            //        InvokeAsync(StateHasChanged);
            //    }
            //});

            //await hubConnection.StartAsync();

            //// Get initial stock from hub
            //if (HubConnection != null)
            //{
            //    try
            //    {
            //        var fullStock = await HubConnection.InvokeAsync<List<StockItem>>("GetStockAsync");
            //        if (fullStock != null)
            //        {
            //            foreach (var stock in fullStock)
            //            {
            //                var existing = catalogItems.FirstOrDefault(ci => ci.Id == stock.ItemId);
            //                if (existing != null)
            //                {
            //                    existing.Total = stock.Total;
            //                    existing.Reserved = stock.Reserved;
            //                }
            //            }
            //            StateHasChanged();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"Error fetching initial stock: {ex.Message}");
            //    }
            //}

            CallRequestRefresh();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private void UpdateStock(StockItem item)
    {
        var existing = catalogItems.Find(ci => ci.Id == item.ItemId);
        if (existing != null)
        {
            existing.Total = item.Total;
            existing.Reserved = item.Reserved;
            InvokeAsync(StateHasChanged);
        }
    }

    private async Task RestockClick(int itemId)
    {
        //if (restockAmounts.TryGetValue(itemId, out var amount))
        //{
        //    if (StockHub != null)
        //    {
        //        await StockHub.RestockAsync(itemId, amount);

        //        restockAmounts[itemId] = 1;
        //    }
        //}
    }

    private async void DetailsClick(int id) => await DetailsComponent.Open(id);
    private async Task CreateClick() => await CreateComponent.Open();
    private async Task EditClick(int id) => await EditComponent.Open(id);
    private async Task DeleteClick(int id) => await DeleteComponent.Open(id);

    private async Task ReloadCatalogItems()
    {
        catalogItems = await CatalogItemService.List();
        foreach (var item in catalogItems)
            restockAmounts[item.Id] = 1;
        StateHasChanged();
    }

    public class StockItem
    {
        public int ItemId { get; set; }
        public int Total { get; set; }
        public int Reserved { get; set; }
    }
}
