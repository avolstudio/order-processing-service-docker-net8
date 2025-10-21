using OrderService.Domain;

namespace OrderService.Behaviours.Discount;

public class BulkDiscountStrategy(int minItems, decimal percentage) : IDiscountStrategy
{
    public decimal Apply(Order order)
    {
        if (order.Items.Count >= minItems)
            return order.TotalAmount * (1 - percentage / 100m);
        return order.TotalAmount;
    }
}