namespace OrderService.DTO;

public sealed class OrderResponse
{
    public int Id { get; init; }
    public int CustomerId { get; init; }
    public IEnumerable<string> Items { get; init; } = Array.Empty<string>();
    public decimal TotalAmount { get; init; }
}
