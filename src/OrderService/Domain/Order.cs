using System.ComponentModel.DataAnnotations;

namespace OrderService.Domain;

public class Order
{
    [Key]
    public int Id { get; set; } 
    public string? IdempotencyKey { get; set; }
    public int CustomerId { get; set; }
    public List<string> Items { get; set; } = new ();
    public decimal TotalAmount { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool Processed { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
