using FluentValidation;
using OrderService.DTO;

namespace OrderService.Validation;

public sealed class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required")
            .GreaterThan(0).WithMessage("CustomerId must be a positive integer");

        RuleFor(x => x.Items)
            .NotNull().WithMessage("Items list is required")
            .Must(list => list.Count > 0).WithMessage("Items list cannot be empty");
    }
}