using ECommerce.Modules.Orders.Entities;
using ECommerce.Modules.Orders.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Contracts.Customers;
using ECommerce.Shared.Contracts.Inventory;
using ECommerce.Shared.IntegrationEvents;
using ECommerce.Shared.Mediator;
using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Orders.Features.PlaceOrder;

public class PlaceOrderHandler : IRequestHandler<PlaceOrderCommand, Result<OrderResponse>>
{
    private readonly OrdersDbContext _db;
    private readonly IMediator _mediator;
    private readonly IIntegrationEventPublisher _integrationEvents;
    private readonly ILogger<PlaceOrderHandler> _logger;

    public PlaceOrderHandler(OrdersDbContext db, IMediator mediator, IIntegrationEventPublisher integrationEvents, ILogger<PlaceOrderHandler> logger)
    {
        _db = db; _mediator = mediator; _integrationEvents = integrationEvents; _logger = logger;
    }

    public async Task<Result<OrderResponse>> Handle(PlaceOrderCommand cmd, CancellationToken ct)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == cmd.UserId && c.Status == "Active", ct);

        if (cart is null || cart.Items.Count == 0)
            return Result<OrderResponse>.Failure("Cart is empty.");

        // Fall back to the customer's saved shipping address when none is provided.
        var shippingAddress = cmd.ShippingAddress;
        if (string.IsNullOrWhiteSpace(shippingAddress))
        {
            var addressResult = await _mediator.Send(new GetCustomerShippingAddressRequest(cmd.UserId), ct);
            if (!addressResult.IsSuccess)
                return Result<OrderResponse>.Failure("No shipping address provided and none on file. Update your profile or supply an address.");

            shippingAddress = addressResult.Value!.ToString();
        }

        // Friendly per-item availability check first...
        foreach (var item in cart.Items)
        {
            var availability = await _mediator.Send(new GetStockAvailabilityRequest(item.ProductId), ct);
            var available = availability.IsSuccess ? availability.Value : 0;
            if (available < item.Quantity)
                return Result<OrderResponse>.Failure($"Insufficient stock for '{item.ProductName}' — {available} available.");
        }

        // ...then the atomic all-or-nothing reservation guards against concurrent checkouts.
        var lines = cart.Items.Select(i => new StockLine(i.ProductId, i.Quantity)).ToList();
        var reservation = await _mediator.Send(new ReserveStockRequest(lines), ct);
        if (!reservation.IsSuccess)
            return Result<OrderResponse>.Failure("Stock levels changed while placing your order. Please try again.");

        var order = Order.Create(cmd.UserId, cart.Items, shippingAddress, cmd.Notes);
        _db.Orders.Add(order);
        cart.Clear();

        var placedEvent = new OrderPlacedEvent(order.Id, order.OrderNumber, cmd.UserId, lines);

        // Point-to-point: queued for exactly one consumer (see Inventory's OrderPlacedQueueConsumer).
        // Enqueued into the same change tracker as the order, so it commits atomically with it below.
        _integrationEvents.Enqueue(placedEvent, MessageKind.PointToPoint, "orders.placed.queue");

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch
        {
            // Compensate: neither the order nor the outbox row was persisted, so hand the reservation back.
            await _mediator.Send(new ReleaseStockRequest(lines), CancellationToken.None);
            throw;
        }

        // Inventory commits the reservation into a real stock deduction. The order is placed and
        // stock is still reserved (no oversell) even if this fails, so a failure here is logged for
        // reconciliation instead of turning an already-successful order into a failed request.
        await _mediator.PublishBestEffort(placedEvent, _logger, $"Order {order.OrderNumber} placed but stock commit failed; reservation left in place", ct);

        return Result<OrderResponse>.Success(MapToResponse(order));
    }

    private static OrderResponse MapToResponse(Order o) => new(
        o.Id, o.OrderNumber, o.UserId,
        o.Status.ToString(), o.TotalAmount,
        o.ShippingAddress, o.Notes,
        o.Items.Select(i => new OrderItemResponse(i.Id, i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.TotalPrice)).ToList(),
        o.CreatedAt, o.UpdatedAt);
}
