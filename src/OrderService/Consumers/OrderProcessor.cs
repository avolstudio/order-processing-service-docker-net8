using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Behaviours.Discount;
using OrderService.Data;
using OrderService.Domain;
using OrderService.Messaging;
using OrderService.Misc;
using Prometheus;

namespace OrderService.Consumers;

public sealed class OrderProcessor : IConsumer<OrderSubmitted>
{
    private readonly OrdersDbContext _db;
    private readonly ILogger<OrderProcessor> _logger;
    private readonly IDiscountStrategy _discountStrategy;
    private readonly IPriceCalculator _priceCalculator;

    private static readonly Counter OrdersProcessed = Metrics
        .CreateCounter("orders_processed_total", "Number of orders processed successfully"); 
    private static readonly Gauge TotalAmountOfOrders = Metrics
        .CreateGauge("orders_total_amount", "Sum of total orders amount of orders processed successfully");
    
    public OrderProcessor(OrdersDbContext db, ILogger<OrderProcessor> logger, IDiscountStrategy discountStrategy,IPriceCalculator priceCalculator)
    {
        _db = db;
        _logger = logger;
        _discountStrategy = discountStrategy;
        _priceCalculator = priceCalculator;
    }

    public async Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing order {OrderId}", message.OrderId);

        await ProcessOrder(message);
    }

    private async Task ProcessOrder(OrderSubmitted message)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == message.OrderId && o.Processed == false);

        if (order == null)
        {
            _logger.LogInformation("Order {OrderId} doesn't exist", message.OrderId);
            return;
        }

        _priceCalculator.CalculateTotal(ref order);
        order.TotalAmount = ApplyDiscount(order);
        order.Processed = true;
        order.ProcessedAt = DateTime.UtcNow;
            
        await _db.SaveChangesAsync();
        _logger.LogInformation("Order {OrderId} processed", message.OrderId);
        
        OrdersProcessed.Inc();
        TotalAmountOfOrders.Inc((double)order.TotalAmount);
    }

    private decimal ApplyDiscount(Order order)
    {
        return _discountStrategy.Apply(order);
    }
}
