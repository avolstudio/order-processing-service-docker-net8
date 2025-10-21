using OrderService.Domain;

namespace OrderService.Behaviours.Discount;

public interface IDiscountStrategy
{
    decimal Apply(Order order);
}