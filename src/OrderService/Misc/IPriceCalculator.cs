using OrderService.Domain;

namespace OrderService.Misc;

public interface IPriceCalculator
{
    void CalculateTotal(ref Order order);
}