using FluentValidation;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms.Validators;

[RegisterScoped]
public class ProductContextDtoValidator : AbstractValidator<ProductContextDto>
{
    public ProductContextDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("ProductId must be greater than 0");

        RuleFor(x => x.DailyRate)
            .GreaterThan(0)
            .WithMessage("DailyRate must be greater than 0");
    }
}

[RegisterScoped]
public class ProductContextDtoWithCycleTimeValidator : ProductContextDtoValidator
{
    public ProductContextDtoWithCycleTimeValidator()
    {
        RuleFor(x => x.CycleTime)
            .NotNull()
            .WithMessage("CycleTime is required")
            .GreaterThan(0)
            .WithMessage("CycleTime must be greater than 0");
    }
}

[RegisterScoped]
public class ProductContextDtoWithWorkstationCapacityValidator : ProductContextDtoValidator
{
    public ProductContextDtoWithWorkstationCapacityValidator()
    {
        RuleFor(x => x.WorkstationCapacity)
            .NotNull()
            .WithMessage("WorkstationCapacity is required")
            .GreaterThan(0)
            .WithMessage("WorkstationCapacity must be greater than 0");
    }
}