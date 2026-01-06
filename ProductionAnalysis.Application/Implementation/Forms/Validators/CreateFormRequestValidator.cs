using FluentValidation;
using ProductionAnalysis.Application.Converters;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms.Validators;

[RegisterScoped]
public class CreateFormRequestValidator : AbstractValidator<CreateFormRequest>
{
    public CreateFormRequestValidator(
        ProductContextDtoWithCycleTimeValidator productWithCycleTimeValidator,
        ProductContextDtoWithWorkstationCapacityValidator productWithWorkstationCapacityValidator,
        OperationContextDtoValidator operationValidator)
    {
        RuleFor(x => x.PaType)
            .IsInEnum()
            .WithMessage("PaType must be a valid value");

        RuleFor(x => x.ShiftId)
            .GreaterThan(0)
            .WithMessage("ShiftId must be greater than 0");

        // Валидация для SingleProductWithCycleTime
        When(x => x.PaType.ToDomain() == PaType.SingleProductWithCycleTime, () =>
        {
            RuleFor(x => x.Product)
                .NotNull()
                .WithMessage($"Product is required for {PaType.SingleProductWithCycleTime}")
                .SetValidator(productWithCycleTimeValidator);

            RuleFor(x => x.Products)
                .Null()
                .WithMessage($"Products must be null for {PaType.SingleProductWithCycleTime}");

            RuleFor(x => x.Operation)
                .Null()
                .WithMessage($"Operation must be null for {PaType.SingleProductWithCycleTime}");
        });

        // Валидация для SingleProductWithWorkstationCapacity
        When(x => x.PaType.ToDomain() == PaType.SingleProductWithWorkstationCapacity, () =>
        {
            RuleFor(x => x.Product)
                .NotNull()
                .WithMessage($"Product is required for {PaType.SingleProductWithWorkstationCapacity}")
                .SetValidator(productWithWorkstationCapacityValidator);

            RuleFor(x => x.Products)
                .Null()
                .WithMessage($"Products must be null for {PaType.SingleProductWithWorkstationCapacity}");

            RuleFor(x => x.Operation)
                .Null()
                .WithMessage($"Operation must be null for {PaType.SingleProductWithWorkstationCapacity}");
        });

        // Валидация для MultipleProductsWithCycleTime
        When(x => x.PaType.ToDomain() == PaType.MultipleProductsWithCycleTime, () =>
        {
            RuleFor(x => x.Products)
                .NotNull()
                .WithMessage($"Products are required for {PaType.MultipleProductsWithCycleTime}")
                .NotEmpty()
                .WithMessage($"Products collection must not be empty for {PaType.MultipleProductsWithCycleTime}");

            RuleForEach(x => x.Products!)
                .SetValidator(productWithCycleTimeValidator)
                .When(x => x.Products != null);

            RuleFor(x => x.Product)
                .Null()
                .WithMessage($"Product must be null for {PaType.MultipleProductsWithCycleTime}");

            RuleFor(x => x.Operation)
                .Null()
                .WithMessage($"Operation must be null for {PaType.MultipleProductsWithCycleTime}");
        });

        // Валидация для LessThanOnePerHour
        When(x => x.PaType.ToDomain() == PaType.LessThanOnePerHour, () =>
        {
            RuleFor(x => x.Operation)
                .NotNull()
                .WithMessage($"Operation is required for {PaType.LessThanOnePerHour}")
                .SetValidator(operationValidator);

            RuleFor(x => x.Product)
                .Null()
                .WithMessage($"Product must be null for {PaType.LessThanOnePerHour}");

            RuleFor(x => x.Products)
                .Null()
                .WithMessage($"Products must be null for {PaType.LessThanOnePerHour}");
        });

        // Валидация для LessThanOnePerShift
        When(x => x.PaType.ToDomain() == PaType.LessThanOnePerShift, () =>
        {
            RuleFor(x => x.Operation)
                .NotNull()
                .WithMessage($"Operation is required for {PaType.LessThanOnePerShift}")
                .SetValidator(operationValidator);

            RuleFor(x => x.Product)
                .Null()
                .WithMessage($"Product must be null for {PaType.LessThanOnePerShift}");

            RuleFor(x => x.Products)
                .Null()
                .WithMessage($"Products must be null for {PaType.LessThanOnePerShift}");
        });
    }
}