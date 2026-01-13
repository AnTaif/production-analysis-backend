using FluentValidation;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms.Validators;

[RegisterScoped]
public class OperationOrProductContextRequestValidator : AbstractValidator<OperationOrProductContextRequest>
{
    public OperationOrProductContextRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x.OperationId.HasValue || x.ProductId.HasValue)
            .WithMessage("Either OperationId or ProductId must be set");

        RuleFor(x => x)
            .Must(x => !(x.OperationId.HasValue && x.ProductId.HasValue))
            .WithMessage("OperationId and ProductId cannot both be set");

        When(x => x.OperationId.HasValue, () =>
        {
            RuleFor(x => x.OperationId)
                .GreaterThan(0)
                .WithMessage("OperationId must be greater than 0");
        });

        When(x => x.ProductId.HasValue, () =>
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0)
                .WithMessage("ProductId must be greater than 0");
        });
    }
}