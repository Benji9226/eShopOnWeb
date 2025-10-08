using BasketService.DTOs;
using BasketService.Enitites;
using Microsoft.AspNetCore.Mvc;

namespace BasketService;

[ApiController]
[Route("/api/[controller]")]
public class BasketController(BasketRepository basketRepository) : ControllerBase
{
    [HttpGet("{buyerId}")]
    public ActionResult<Basket> GetOrCreateBasket(string buyerId)
    {
        var basket = basketRepository.GetOrCreateBasketByUsername(buyerId);
        return Ok(basket);
    }
    
    [HttpGet("count/{buyerId}")]
    public async Task<ActionResult<int>> GetBasketItemCount(string buyerId)
    {
        var count = await basketRepository.CountTotalBasketItems(buyerId);
        return Ok(count);
    }
    
    [HttpPost("addItem")]
    public ActionResult<Basket> AddItemToBasket(AddBasketItemDto addBasketItemDto)
    {
        var basket = basketRepository.GetOrCreateBasketByUsername(addBasketItemDto.Username);
        basket.AddItem(addBasketItemDto.CatalogItemId, addBasketItemDto.Price, addBasketItemDto.Quantity);
        
        basketRepository.Update(basket);

        return Ok(basket);
    }
    
    [HttpPut("setQuantities")]
    public ActionResult<Basket> SetQuantities(UpdateQuantitiesDto dto)
    {
        var basket = basketRepository.Find(dto.BasketId);
        if (basket == null) return NotFound();

        foreach (var item in basket.Items)
        {
            if (dto.Quantities.TryGetValue(item.Id.ToString(), out var quantity))
            {
                //todo logger if (_logger != null) _logger.LogInformation($"Updating quantity of item ID:{item.Id} to {quantity}.");
                item.SetQuantity(quantity);
            }
        }
        basket.RemoveEmptyItems();
        basketRepository.Update(basket);
        return basket;
    }
    
}
