namespace OrderService.DTO;

public record CreateOrderRequest(int CustomerId, List<string> Items);