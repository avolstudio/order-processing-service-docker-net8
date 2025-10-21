using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OrderService.Filters;

public class RequireIdempotencyKeyAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue("Idempotency-Key", out var key) 
            || string.IsNullOrWhiteSpace(key))
        {
            context.Result = new BadRequestObjectResult(new { error = "Idempotency-Key header is required" });
        }
    }
}