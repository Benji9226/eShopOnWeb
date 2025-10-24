namespace Microsoft.eShopWeb.Web.Cache;

public class StockCache
{
    private readonly Dictionary<int, StockItem> _stocks = new();
    private readonly object _lock = new();

    public void Update(int itemId, int total, int reserved)
    {
        lock (_lock)
        {
            _stocks[itemId] = new StockItem { ItemId = itemId, Total = total, Reserved = reserved };
        }
    }

    public StockItem? Get(int itemId)
    {
        lock (_lock)
        {
            _stocks.TryGetValue(itemId, out var stock);
            return stock;
        }
    }

    public IReadOnlyCollection<StockItem> GetAll()
    {
        lock (_lock)
        {
            return _stocks.Values.ToList().AsReadOnly();
        }
    }
}

public class StockItem
{
    public int ItemId { get; set; }
    public int Total { get; set; }
    public int Reserved { get; set; }
}
