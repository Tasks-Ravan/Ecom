namespace ECommerce.Api.Features.Checkout;

public sealed record CheckoutItemRequest(Guid ProductId, int Quantity);

public sealed record CheckoutRequest(IReadOnlyList<CheckoutItemRequest> Items, bool SimulateDecline, string Language);

public sealed record CheckoutResponse(Guid OrderId, string OrderNumber, string Status, string PaymentProvider,
    decimal TotalAmount, string Currency, string Message, DateTimeOffset CreatedAt);

public sealed record OrderLineResponse(Guid ProductId, string Sku, string ProductName, decimal UnitPrice,
    int Quantity, decimal LineTotal);

public sealed record OrderDetailsResponse(Guid OrderId, string OrderNumber, string Status, string PaymentProvider,
    decimal TotalAmount, string Currency, DateTimeOffset CreatedAt, IReadOnlyList<OrderLineResponse> Items);
