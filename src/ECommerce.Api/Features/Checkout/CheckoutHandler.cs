using System.Data;
using ECommerce.Api.Common;
using ECommerce.Api.Domain;
using ECommerce.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ECommerce.Api.Features.Checkout;

public sealed class CheckoutHandler(AppDbContext db, IOptions<SandboxPaymentOptions> paymentOptions,
    IHostEnvironment environment, ILogger<CheckoutHandler> logger)
{
    public async Task<OrderDetailsResponse?> GetOrderAsync(Guid userId, Guid orderId, CancellationToken cancellationToken)
    {
        var order = await db.Orders.AsNoTracking().Include(value => value.Items)
            .SingleOrDefaultAsync(value => value.Id == orderId && value.UserId == userId, cancellationToken);
        return order is null ? null : new OrderDetailsResponse(order.Id, order.OrderNumber, order.Status.ToString(),
            order.PaymentProvider, order.TotalAmount, order.Currency, order.CreatedAt,
            order.Items.OrderBy(item => item.Sku).Select(item => new OrderLineResponse(item.ProductId, item.Sku,
                item.ProductName, item.UnitPrice, item.Quantity, item.LineTotal)).ToArray());
    }

    public async Task<CatalogResult<CheckoutResponse>> HandleAsync(Guid userId, CheckoutRequest request,
        CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment() || !paymentOptions.Value.Enabled)
            return CatalogResult<CheckoutResponse>.NotFound("Sandbox checkout is disabled.");

        var productIds = request.Items.Select(item => item.ProductId).Order().ToArray();
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        var products = await db.Products.FromSqlInterpolated($"""
            SELECT * FROM "Products"
            WHERE "Id" = ANY ({productIds})
            ORDER BY "Id"
            FOR UPDATE
            """).ToListAsync(cancellationToken);

        if (products.Count != productIds.Length)
            return CatalogResult<CheckoutResponse>.Conflict("One or more products are no longer available.");

        var lines = new List<OrderItem>(request.Items.Count);
        decimal total = 0;
        foreach (var item in request.Items)
        {
            var product = products.Single(product => product.Id == item.ProductId);
            if (!product.IsActive)
                return CatalogResult<CheckoutResponse>.Conflict($"Product {product.Sku} is no longer available.");
            if (product.StockQuantity < item.Quantity)
                return CatalogResult<CheckoutResponse>.Conflict($"Not enough stock for product {product.Sku}.");

            var (_, translation) = ProductTranslationResolver.Resolve(product.Translations, request.Language);
            var lineTotal = decimal.Round(product.Price * item.Quantity, 2, MidpointRounding.AwayFromZero);
            total += lineTotal;
            lines.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Sku = product.Sku,
                ProductName = translation.Name,
                UnitPrice = product.Price,
                Quantity = item.Quantity,
                LineTotal = lineTotal
            });
        }

        var now = DateTimeOffset.UtcNow;
        var paid = !request.SimulateDecline;
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = $"ORD-{now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..32].ToUpperInvariant(),
            UserId = userId,
            Status = paid ? OrderStatus.Paid : OrderStatus.PaymentDeclined,
            PaymentProvider = "Sandbox",
            PaymentReference = $"SBX-{Guid.NewGuid():N}",
            Currency = "AZN",
            TotalAmount = total,
            CreatedAt = now,
            PaidAt = paid ? now : null,
            Items = lines
        };

        if (paid)
            foreach (var item in request.Items)
                products.Single(product => product.Id == item.ProductId).StockQuantity -= item.Quantity;

        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        logger.LogInformation("Sandbox order {OrderNumber} for user {UserId} completed with status {Status}",
            order.OrderNumber, userId, order.Status);
        var message = paid ? "Sandbox payment approved. No real payment was collected." :
            "Sandbox payment declined. No payment was collected and stock was not changed.";
        return CatalogResult<CheckoutResponse>.Created(new CheckoutResponse(order.Id, order.OrderNumber,
            order.Status.ToString(), order.PaymentProvider, order.TotalAmount, order.Currency, message, order.CreatedAt));
    }
}
