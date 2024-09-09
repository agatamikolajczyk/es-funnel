namespace Funnel.Core.Carts;

public class Item
{
    public string Index { get; private set; }
    public int Quantity { get; private set; }

    public Item(string index, int quantity)
    {
        Index = index;
        Quantity = quantity;
    }
}