using MediatR;
using Microsoft.eShopWeb.Web.Clients;
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

        services.Configure<BaseUrls>(configuration.GetSection("baseUrls"));

        services.AddHttpClient<ICatalogApiClient, CatalogApiClient>((sp, client) =>
        {
            var baseUrls = sp.GetRequiredService<IOptions<BaseUrls>>().Value;
            client.BaseAddress = new Uri(baseUrls.CatalogMicroservice);
        });

        services.AddScoped<CatalogViewModelService>();
        services.AddScoped<ICatalogViewModelService, CachedCatalogViewModelService>();

        services.AddScoped<IBasketViewModelService, BasketViewModelService>();
        services.AddScoped<ICatalogItemViewModelService, CatalogItemViewModelService>();
        services.Configure<CatalogSettings>(configuration);

        return services;
    }
}

public class BaseUrls
{
    public string ApiBase { get; set; } = "";
    public string WebBase { get; set; } = "";
    public string CatalogMicroservice { get; set; } = "";
}
