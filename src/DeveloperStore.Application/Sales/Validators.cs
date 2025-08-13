using DeveloperStore.Application.DTOs;
using FluentValidation;

namespace DeveloperStore.Application.Sales;

public class SaleCreateValidator : AbstractValidator<SaleCreateDto>
{
    public SaleCreateValidator()
    {
        RuleFor(x => x.Number).NotEmpty();
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.CustomerName).NotEmpty();
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.BranchName).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new SaleItemInValidator());
    }
}

public class SaleUpdateValidator : AbstractValidator<SaleUpdateDto>
{
    public SaleUpdateValidator()
    {
        RuleFor(x => x.Number).NotEmpty();
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.CustomerName).NotEmpty();
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.BranchName).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new SaleItemInValidator());
    }
}

public class SaleItemInValidator : AbstractValidator<SaleItemIn>
{
    public SaleItemInValidator()
    {
        RuleFor(i => i.ProductId).GreaterThan(0);
        RuleFor(i => i.ProductName).NotEmpty();
        RuleFor(i => i.Quantity).GreaterThan(0);
        RuleFor(i => i.UnitPrice).GreaterThan(0);
    }
}
