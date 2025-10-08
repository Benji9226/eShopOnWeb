using BasketService.Data;
using BasketService.Enitites;
using Microsoft.EntityFrameworkCore;

namespace BasketService;

public class BasketRepository(BasketContext context)
{
    public Basket GetOrCreateBasketByUsername(string buyerId)
    {
        var basket =  context.Baskets
            .Where(b => b.BuyerId == buyerId)
            .Include(b => b.Items)
            .FirstOrDefault();

        return basket ?? CreateBasket(new Basket(buyerId));
    }

    private Basket CreateBasket(Basket basket)
    {
        var addedBasket = context.Baskets.Add(basket);
        context.SaveChanges();

        return addedBasket.Entity;
    }

    public async Task<int> CountTotalBasketItems(string buyerId)
    {
        return await context.Baskets
            .Where(b => b.BuyerId == buyerId)
            .SelectMany(b => b.Items)
            .SumAsync(i => i.Quantity);
    }


    public void Update(Basket basket)
    {
        context.Baskets.Update(basket);

        context.SaveChanges();
    }

    public Basket? Find(int basketId)
    {
        var basket =  context.Baskets
            .Where(b => b.Id == basketId)
            .Include(b => b.Items)
            .FirstOrDefault();
        
        return basket;
    }
}
