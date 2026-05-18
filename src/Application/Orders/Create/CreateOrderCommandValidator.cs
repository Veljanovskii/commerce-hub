using FluentValidation;

namespace Application.Orders.Create;

internal sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer is required");

        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("Shipping address is required");

        RuleFor(x => x.ShippingCity)
            .NotEmpty().WithMessage("Shipping city is required");

        RuleFor(x => x.ShippingCountry)
            .NotEmpty().WithMessage("Shipping country is required");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one order item is required");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("Product is required");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero");

            item.RuleFor(i => i.Discount)
                .GreaterThanOrEqualTo(0).WithMessage("Discount cannot be negative");
        });
    }
}
