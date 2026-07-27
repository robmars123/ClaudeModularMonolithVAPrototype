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
    private readonly PlaceOrderIdempotencyGuard _idempotency;
    private readonly ILogger<PlaceOrderHandler> _logger;

    public PlaceOrderHandler(OrdersDbContext db, IMediator mediator, IIntegrationEventPublisher integrationEvents, PlaceOrderIdempotencyGuard idempotency, ILogger<PlaceOrderHandler> logger)
    {
        _db = db; _mediator = mediator; _integrationEvents = integrationEvents; _idempotency = idempotency; _logger = logger;
    }

    public async Task<Result<OrderResponse>> Handle(PlaceOrderCommand cmd, CancellationToken ct)
    {
        Guid? lease = _idempotency.TryAcquire(cmd.UserId);
        if (lease is null)
            return Result<OrderResponse>.Failure("A request to place this order is already in progress. Please wait a moment and try again.");

        try
        {
            Cart? cart = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == cmd.UserId && c.Status == "Active", ct);

            if (cart is null || cart.Items.Count == 0)
                return Result<OrderResponse>.Failure("Cart is empty.");

            Result<string> addressResult = await ResolveShippingAddressAsync(cmd, ct);
            if (!addressResult.IsSuccess)
                return Result<OrderResponse>.Failure(addressResult.Error!);

            Result availabilityCheck = await CheckAvailabilityAsync(cart, ct);
            if (!availabilityCheck.IsSuccess)
                return Result<OrderResponse>.Failure(availabilityCheck.Error!);

            return await ReserveAndPersistOrderAsync(cmd, cart, addressResult.Value!, ct);
        }
        finally
        {
            _idempotency.Release(cmd.UserId, lease.Value);
        }
    }

    // Falls back to the customer's saved shipping address when none is provided.
    private async Task<Result<string>> ResolveShippingAddressAsync(PlaceOrderCommand cmd, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(cmd.ShippingAddress))
            return Result<string>.Success(cmd.ShippingAddress);

        Result<CustomerShippingAddressDto> addressResult = await _mediator.Send(new GetCustomerShippingAddressRequest(cmd.UserId), ct);
        return addressResult.IsSuccess
            ? Result<string>.Success(addressResult.Value!.ToString())
            : Result<string>.Failure("No shipping address provided and none on file. Update your profile or supply an address.");
    }

    // Friendly per-item availability check — the atomic reservation below is what actually guards against concurrent checkouts.
    private async Task<Result> CheckAvailabilityAsync(Cart cart, CancellationToken ct)
    {
        foreach (CartItem item in cart.Items)
        {
            Result<int> availability = await _mediator.Send(new GetStockAvailabilityRequest(item.ProductId), ct);
            int available = availability.IsSuccess ? availability.Value : 0;
            if (available < item.Quantity)
                return Result.Failure($"Insufficient stock for '{item.ProductName}' — {available} available.");
        }

        return Result.Success();
    }

    private async Task<Result<OrderResponse>> ReserveAndPersistOrderAsync(PlaceOrderCommand cmd, Cart cart, string shippingAddress, CancellationToken ct)
    {
        List<StockLine> lines = cart.Items.Select(i => new StockLine(i.ProductId, i.Quantity)).ToList();
        Result reservation = await _mediator.Send(new ReserveStockRequest(lines), ct);
        if (!reservation.IsSuccess)
            return Result<OrderResponse>.Failure("Stock levels changed while placing your order. Please try again.");

        Order order = Order.Create(cmd.UserId, cart.Items, shippingAddress, cmd.Notes);
        _db.Orders.Add(order);
        cart.Clear();

        // Best-effort — a missing contact just means the email consumer skips sending, never blocks the
        // order. Explicitly caught (not just a Result failure check): an exception here must not escape
        // and leave the reservation above uncompensated — this lookup is genuinely optional.
        string? customerEmail = null;
        string? customerName = null;
        try
        {
            Result<CustomerContactDto> contact = await _mediator.Send(new GetCustomerContactRequest(cmd.UserId), ct);
            if (contact.IsSuccess)
            {
                customerEmail = contact.Value!.Email;
                customerName = contact.Value!.FullName;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not resolve customer contact for order notification; proceeding without it.");
        }

        OrderPlacedEvent placedEvent = new OrderPlacedEvent(order.Id, order.OrderNumber, cmd.UserId, lines)
        {
            CustomerEmail = customerEmail,
            CustomerName = customerName,
        };

        // Point-to-point: queued for exactly one consumer (see Inventory's OrderPlacedQueueConsumer).
        // Enqueued into the same change tracker as the order, so it commits atomically with it below.
        _integrationEvents.Enqueue(placedEvent, MessageKind.PointToPoint, "orders.placed.queue");
        // Pub/sub copy for out-of-process notification subscribers (e.g. EmailWorker's order-confirmation
        // consumer) — independent of the point-to-point queue Inventory consumes for stock commit.
        _integrationEvents.Enqueue(placedEvent, MessageKind.PubSub, "order.placed");

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch
        {
            // Compensate: neither the order nor the outbox rows were persisted, so hand the reservation back.
            await _mediator.Send(new ReleaseStockRequest(lines), CancellationToken.None);
            throw;
        }

        // Inventory commits the reservation into a real stock deduction. The order is placed and
        // stock is still reserved (no oversell) even if this fails, so a failure here is logged for
        // reconciliation instead of turning an already-successful order into a failed request.
        await _mediator.PublishBestEffort(placedEvent, _logger, $"Order {order.OrderNumber} placed but stock commit failed; reservation left in place", ct);

        return Result<OrderResponse>.Success(OrderResponse.FromEntity(order));
    }
}
