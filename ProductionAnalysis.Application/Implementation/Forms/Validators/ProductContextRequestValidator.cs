using FluentValidation;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms.Validators;

[RegisterScoped]
public class ProductContextRequestValidator : AbstractValidator<ProductContextRequest>
{
    public ProductContextRequestValidator()
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
public class ProductContextRequestWithCycleTimeValidator : ProductContextRequestValidator
{
    public ProductContextRequestWithCycleTimeValidator()
    {
        RuleFor(x => x.CycleTime)
            .NotNull()
            .WithMessage("CycleTime is required")
            .GreaterThan(0)
            .WithMessage("CycleTime must be greater than 0");
    }
}

[RegisterScoped]
public class ProductContextRequestWithWorkstationCapacityValidator : ProductContextRequestValidator
{
    public ProductContextRequestWithWorkstationCapacityValidator()
    {
        RuleFor(x => x.WorkstationCapacity)
            .NotNull()
            .WithMessage("WorkstationCapacity is required")
            .GreaterThan(0)
            .WithMessage("WorkstationCapacity must be greater than 0");
    }
}