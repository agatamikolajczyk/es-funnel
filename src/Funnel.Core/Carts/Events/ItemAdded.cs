namespace Funnel.Core.Carts.Events;

public record ItemAdded(Guid CartId, string Index, int Quantity);