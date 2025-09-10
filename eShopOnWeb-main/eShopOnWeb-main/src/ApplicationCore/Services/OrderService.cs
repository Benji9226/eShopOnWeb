using System.Linq;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.eShopWeb.ApplicationCore.Contracts.Orders;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;

namespace Microsoft.eShopWeb.ApplicationCore.Services;

public class OrderService : IOrderService
{
    private readonly IRepository<Basket> _basketRepository;
    private readonly IRepository<CatalogItem> _itemRepository;
    private readonly IUriComposer _uriComposer;
    private readonly IOrderServiceClient _orderServiceClient;

    public OrderService(
        IRepository<Basket> basketRepository,
        IRepository<CatalogItem> itemRepository,
        IUriComposer uriComposer,
        IOrderServiceClient orderServiceClient)
    {
        _basketRepository = basketRepository;
        _itemRepository = itemRepository;
        _uriComposer = uriComposer;
        _orderServiceClient = orderServiceClient;
    }

    public async Task CreateOrderAsync(int basketId, Address shippingAddress)
    {
        var basketSpec = new BasketWithItemsSpecification(basketId);
        var basket = await _basketRepository.FirstOrDefaultAsync(basketSpec);

        Guard.Against.Null(basket, nameof(basket));
        Guard.Against.EmptyBasketOnCheckout(basket.Items);

        var catalogItemsSpecification = new CatalogItemsSpecification(basket.Items.Select(i => i.CatalogItemId).ToArray());
        var catalogItems = await _itemRepository.ListAsync(catalogItemsSpecification);

        var items = basket.Items.Select(basketItem =>
        {
            var catalogItem = catalogItems.First(c => c.Id == basketItem.CatalogItemId);
            var itemOrdered = new CatalogItemOrdered(catalogItem.Id, catalogItem.Name, _uriComposer.ComposePicUri(catalogItem.PictureUri));
            return new OrderItemDto
            {
                ItemOrdered_CatalogItemId = itemOrdered.CatalogItemId,
                ItemOrdered_ProductName = itemOrdered.ProductName,
                ItemOrdered_PictureUri = itemOrdered.PictureUri,
                UnitPrice = basketItem.UnitPrice,
                Units = basketItem.Quantity
            };
        }).ToList();

        var createOrderDto = new CreateOrderDto
        {
            BuyerId = basket.BuyerId,
            ShipToAddress_Street = shippingAddress.Street,
            ShipToAddress_City = shippingAddress.City,
            ShipToAddress_State = shippingAddress.State,
            ShipToAddress_Country = shippingAddress.Country,
            ShipToAddress_ZipCode = shippingAddress.ZipCode,
            Items = items
        };

        await _orderServiceClient.CreateOrderAsync(createOrderDto);
    }
}

