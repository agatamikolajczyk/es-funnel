namespace Funnel.Core.Carts.Events;

public record CartCreated(Guid CartId, string Username);