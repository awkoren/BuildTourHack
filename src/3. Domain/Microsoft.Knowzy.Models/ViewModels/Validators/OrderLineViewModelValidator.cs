﻿using FluentValidation;

namespace Microsoft.Knowzy.Models.ViewModels.Validators
{
    public class OrderLineViewModelValidator : AbstractValidator<OrderLineViewModel>
    {
        public OrderLineViewModelValidator()
        {
            RuleFor(orderLine => orderLine.Quantity)
                .NotEmpty()
                .WithMessage("A number between 1 - 999 is required")
                .GreaterThan(0)
                .WithMessage("At least one item is mandatory")
                .LessThanOrEqualTo(999)
                .WithMessage("Not more than 999 can be purchased")
                .Must(quantity => quantity is int)
                .WithMessage("A number between 1 - 999 is required");
        }
    }
}
