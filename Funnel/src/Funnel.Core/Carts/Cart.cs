using Funnel.Core.Carts.Events;
using Funnel.EventsStore;

namespace Funnel.Core.Carts;

public class Cart : Aggregate
{
    public string Username { get; private set; }
    private List<Item> _items = new List<Item>();
    public IReadOnlyCollection<Item> Items => _items;

    private Cart()
    {
    }

    public Cart(Guid id, string username)
    {
        var @event = new CartCreated(id, username);

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(CartCreated @event)
    {
        Id = @event.CartId;
        Username = @event.Username;
    }

    public void Apply(ItemAdded @event)
    {
        _items.Add(new Item(@event.Index, @event.Quantity));
    }

    public void AddNewItem(string index, int quantity)
    {
        var @event = new ItemAdded(Id, index, quantity);

        Enqueue(@event);
        Apply(@event);
    }
}