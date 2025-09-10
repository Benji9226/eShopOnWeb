using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Contracts.Orders;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;

public class OrderService : IOrderService
{
    private readonly IOrderServiceClient _orderServiceClient;
    private readonly IRepository<Basket> _basketRepository;
    private readonly IRepository<CatalogItem> _itemRepository;
    private readonly IUriComposer _uriComposer;

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
        var basket = await _basketRepository.FirstOrDefaultAsync(new BasketWithItemsSpecification(basketId));
        if (basket == null || !basket.Items.Any()) throw new InvalidOperationException("Basket is empty or not found.");

        var catalogItems = await _itemRepository.ListAsync(new CatalogItemsSpecification(basket.Items.Select(i => i.CatalogItemId).ToArray()));

        var items = basket.Items.Select(basketItem =>
        {
            var catalogItem = catalogItems.First(c => c.Id == basketItem.CatalogItemId);
            return new OrderItemDto
            {
                ItemOrdered_CatalogItemId = catalogItem.Id,
                ItemOrdered_ProductName = catalogItem.Name,
                ItemOrdered_PictureUri = _uriComposer.ComposePicUri(catalogItem.PictureUri),
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
