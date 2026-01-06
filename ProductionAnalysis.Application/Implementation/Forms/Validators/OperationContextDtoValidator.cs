using FluentValidation;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms.Validators;

[RegisterScoped]
public class OperationContextDtoValidator : AbstractValidator<OperationContextDto>
{
    public OperationContextDtoValidator()
    {
        RuleFor(x => x.OperationId)
            .GreaterThan(0)
            .WithMessage("OperationId must be greater than 0");
    }
}