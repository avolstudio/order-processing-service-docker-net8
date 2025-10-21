using FluentValidation;
using FluentValidation.AspNetCore;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Behaviours.Discount;
using OrderService.Consumers;
using OrderService.Data;
using OrderService.Misc;
using OrderService.Validation;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

var postgresConn = builder.Configuration.GetConnectionString("Postgres") ??
    builder.Configuration["ConnectionStrings:Postgres"] ??
    "Host=postgres;Port=5432;Database=ordersdb;Username=postgres;Password=postgres";

var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "rabbitmq";

builder.Services.AddDbContext<OrdersDbContext>(opts =>
    opts.UseNpgsql(postgresConn));

builder.Services.AddScoped<IDiscountStrategy, BulkDiscountStrategy>(serviceProvider =>
    new BulkDiscountStrategy(2,10));
builder.Services.AddTransient<IPriceCalculator, PriceCalculator>();

var rabbitConfig = builder.Configuration.GetSection("RabbitMq");

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderProcessor>();

    configurator.UsingRabbitMq((context, factoryConfigurator) =>
    {
        factoryConfigurator.Host(
            rabbitConfig["Host"] ?? "rabbitmq",
            rabbitConfig["VirtualHost"] ?? "/",
            hostConfigurator =>
            {
                hostConfigurator.Username(rabbitConfig["Username"] ?? "guest");
                hostConfigurator.Password(rabbitConfig["Password"] ?? "guest");
            });

        factoryConfigurator.ReceiveEndpoint(
            rabbitConfig["QueueName"] ?? "order-submitted-queue",
            endpointConfigurator =>
            {
                endpointConfigurator.ConfigureConsumer<OrderProcessor>(context);
                
                endpointConfigurator.UseMessageRetry(r =>
                {
                    r.Interval(3, TimeSpan.FromSeconds(5)); 
                });

                endpointConfigurator.Durable = true; 
                endpointConfigurator.AutoDelete = false;
            });
    });
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

var app = builder.Build();

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapMetrics(); // /metrics
});

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR: {ex}");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
});

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
