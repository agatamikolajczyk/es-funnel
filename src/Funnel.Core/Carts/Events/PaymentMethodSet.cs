namespace Funnel.Core.Carts.Events;

public record PaymentMethodSet(Guid CartId, string PaymentMethod);