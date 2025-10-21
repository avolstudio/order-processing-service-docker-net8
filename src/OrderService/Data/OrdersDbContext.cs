using Microsoft.EntityFrameworkCore;
using OrderService.Domain;

namespace OrderService.Data;

public sealed class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
    public DbSet<Order> Orders => Set<Order>();
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(o => o.IdempotencyKey)
                .IsUnique();
        });
    }
}
