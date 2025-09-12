using Ardalis.GuardClauses;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Web.Clients;
using Microsoft.eShopWeb.Web.DTOs;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.ViewModels;

namespace Microsoft.eShopWeb.Web.Services;

public class CatalogItemViewModelService : ICatalogItemViewModelService
{
    private readonly ICatalogApiClient _catalogApiClient;

    public CatalogItemViewModelService(ICatalogApiClient catalogApiClient)
    {
        _catalogApiClient = catalogApiClient;
    }

    public async Task UpdateCatalogItem(CatalogItemViewModel viewModel)
    {
        var dto = CatalogItemDTO.ViewModelToDTO(viewModel);
        await _catalogApiClient.UpdateCatalogItemAsync(dto);
    }
}
