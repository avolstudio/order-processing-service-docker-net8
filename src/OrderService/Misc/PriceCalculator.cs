using OrderService.Domain;

namespace OrderService.Misc;

// Ideally you should have separated service that will prevent any frauds from users
// Here we have an example - simple calculations of total cost of order represented by simple class

public sealed class PriceCalculator(decimal itemPrice = 100):IPriceCalculator
{
    public void CalculateTotal(ref Order order)
    {
        if (order.Items.Count == 0)
            return;

        order.TotalAmount = order.Items.Count * itemPrice;
    }
}
