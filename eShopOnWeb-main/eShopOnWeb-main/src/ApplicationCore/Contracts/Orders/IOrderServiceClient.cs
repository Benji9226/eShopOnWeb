using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Contracts.Orders;

public interface IOrderServiceClient
{
    Task CreateOrderAsync(CreateOrderDto order);
    Task<OrderReadDto> GetOrderByIdAsync(int orderId);
    Task<List<OrderReadDto>> GetOrdersForUserAsync(string buyerId);
}
