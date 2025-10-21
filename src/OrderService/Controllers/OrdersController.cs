using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using OrderService.Data;
using OrderService.Domain;
using OrderService.DTO;
using OrderService.Filters;
using OrderService.Messaging;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController(OrdersDbContext db, IPublishEndpoint publish, ILogger<OrdersController> logger)
    : ControllerBase
{
    [RequireIdempotencyKey]
    [HttpPost]
    public async Task<IActionResult> Post(CreateOrderRequest req, [FromHeader(Name = "Idempotency-Key")] string? key,CancellationToken cancellationToken)
    {
        var existing = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.IdempotencyKey == key, cancellationToken);
        if (existing != null)
            return Accepted(new { orderId = existing.Id });
        
        var order = new Order
        {
            CustomerId = req.CustomerId,
            Items = req.Items,
            IdempotencyKey = key
        };
        
        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);
        await publish.Publish(new OrderSubmitted(order.Id),cancellationToken);

        logger.LogInformation("Order {OrderId} with items {OrderItems} received, accepted and queued", order.Id, order.Items);
        return Accepted(new { orderId = order.Id });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id,CancellationToken cancellationToken)
    {
        var order = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id,cancellationToken);
        
        if (order == null)
            return NotFound();
        
        var dto = new OrderResponse
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Items = order.Items,
            TotalAmount = order.TotalAmount
        };
        
        return Ok(dto);
    }
}
