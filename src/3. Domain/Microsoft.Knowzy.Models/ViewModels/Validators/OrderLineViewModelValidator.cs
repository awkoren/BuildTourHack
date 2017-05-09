﻿using FluentValidation;

namespace Microsoft.Knowzy.Models.ViewModels.Validators
{
    public class OrderLineViewModelValidator : AbstractValidator<OrderLineViewModel>
    {
        public OrderLineViewModelValidator()
        {
            RuleFor(orderLine => orderLine.Quantity)
                .GreaterThan(0)
                .WithMessage("At least one item is mandatory")
                .LessThanOrEqualTo(999)
                .WithMessage("Not more than 999 can be purchased");
        }
    }
}
