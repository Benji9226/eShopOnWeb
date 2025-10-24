using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using static BlazorAdmin.Pages.CatalogItemPage.List;

namespace BlazorAdmin.Services;

public class StockHubService : IAsyncDisposable
{
    private readonly HubConnection _hub;

    public StockHubService(NavigationManager nav)
    {
        _hub = new HubConnectionBuilder()
            .WithUrl(nav.ToAbsoluteUri("/stockhub"))
            .WithAutomaticReconnect()
            .Build();
    }

    public async Task StartAsync() => await _hub.StartAsync();

    public void OnStockUpdated(Action<StockItem> callback) =>
        _hub.On("StockUpdated", callback);

    public async ValueTask DisposeAsync() => await _hub.DisposeAsync();
}
