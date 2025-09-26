﻿using BlazorShared;
using MediatR;
using Microsoft.eShopWeb.Web.APIClients;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.Services;
using Microsoft.Extensions.Options;

namespace Microsoft.eShopWeb.Web.Configuration;

public static class ConfigureWebServices
{
    public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(typeof(BasketViewModelService).Assembly));

        var configSection = configuration.GetRequiredSection(BaseUrlConfiguration.CONFIG_NAME);
        services.Configure<BaseUrlConfiguration>(configSection);
        var baseUrlConfig = configSection.Get<BaseUrlConfiguration>();

        services.AddHttpClient<ICatalogApiClient, CatalogApiClient>((sp, client) =>
        {
            client.BaseAddress = new Uri(baseUrlConfig.CatalogMicroservice);
        });

        services.AddScoped<CatalogViewModelService>();
        services.AddScoped<ICatalogViewModelService, CachedCatalogViewModelService>();

        services.AddScoped<IBasketViewModelService, BasketViewModelService>();
        services.AddScoped<ICatalogItemViewModelService, CatalogItemViewModelService>();
        services.Configure<CatalogSettings>(configuration);

        return services;
    }
}
