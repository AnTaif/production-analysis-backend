using FluentValidation;
using ProductionAnalysis.Client.Models.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms.Validators;

[RegisterScoped]
public class CreateFormRequestValidator : AbstractValidator<CreateFormRequest>
{
    public CreateFormRequestValidator(
        ProductContextDtoWithCycleTimeValidator productWithCycleTimeValidator,
        ProductContextDtoWithWorkstationCapacityValidator productWithWorkstationCapacityValidator,
        OperationOrProductContextDtoValidator operationOrProductValidator)
    {
        RuleFor(x => x.PaType)
            .IsInEnum()
            .WithMessage("PaType must be a valid value");

        RuleFor(x => x.ShiftId)
            .GreaterThan(0)
            .WithMessage("ShiftId must be greater than 0");

        RuleFor(x => x.AssigneeId)
            .GreaterThan(0)
            .WithMessage("AssigneeId must be greater than 0");

        // Валидация для SingleProductWithCycleTime
        When(x => x.PaType == PaTypeDto.SingleProductWithCycleTime, () =>
        {
            RuleFor(x => x.Product)
                .NotNull()
                .WithMessage($"Product is required for {PaTypeDto.SingleProductWithCycleTime}")
                .SetValidator(productWithCycleTimeValidator);

            RuleFor(x => x.Products)
                .Null()
                .WithMessage($"Products must be null for {PaTypeDto.SingleProductWithCycleTime}");

            RuleFor(x => x.OperationOrProduct)
                .Null()
                .WithMessage($"OperationOrProduct must be null for {PaTypeDto.SingleProductWithCycleTime}");
        });

        // Валидация для SingleProductWithWorkstationCapacity
        When(x => x.PaType == PaTypeDto.SingleProductWithWorkstationCapacity, () =>
        {
            RuleFor(x => x.Product)
                .NotNull()
                .WithMessage($"Product is required for {PaTypeDto.SingleProductWithWorkstationCapacity}")
                .SetValidator(productWithWorkstationCapacityValidator);

            RuleFor(x => x.Products)
                .Null()
                .WithMessage($"Products must be null for {PaTypeDto.SingleProductWithWorkstationCapacity}");

            RuleFor(x => x.OperationOrProduct)
                .Null()
                .WithMessage($"OperationOrProduct must be null for {PaTypeDto.SingleProductWithWorkstationCapacity}");
        });

        // Валидация для MultipleProductsWithCycleTime
        When(x => x.PaType == PaTypeDto.MultipleProductsWithCycleTime, () =>
        {
            RuleFor(x => x.Products)
                .NotNull()
                .WithMessage($"Products are required for {PaTypeDto.MultipleProductsWithCycleTime}")
                .NotEmpty()
                .WithMessage($"Products collection must not be empty for {PaTypeDto.MultipleProductsWithCycleTime}");

            RuleForEach(x => x.Products!)
                .SetValidator(productWithCycleTimeValidator)
                .When(x => x.Products != null);

            RuleFor(x => x.Product)
                .Null()
                .WithMessage($"Product must be null for {PaTypeDto.MultipleProductsWithCycleTime}");

            RuleFor(x => x.OperationOrProduct)
                .Null()
                .WithMessage($"OperationOrProduct must be null for {PaTypeDto.MultipleProductsWithCycleTime}");
        });

        // Валидация для LessThanOnePerHour
        When(x => x.PaType == PaTypeDto.LessThanOnePerHour, () =>
        {
            RuleFor(x => x.OperationOrProduct)
                .NotNull()
                .WithMessage($"OperationOrProduct is required for {PaTypeDto.LessThanOnePerHour}")
                .SetValidator(operationOrProductValidator);

            RuleFor(x => x.Product)
                .Null()
                .WithMessage($"Product must be null for {PaTypeDto.LessThanOnePerHour}");

            RuleFor(x => x.Products)
                .Null()
                .WithMessage($"Products must be null for {PaTypeDto.LessThanOnePerHour}");
        });

        // Валидация для LessThanOnePerShift
        When(x => x.PaType == PaTypeDto.LessThanOnePerShift, () =>
        {
            RuleFor(x => x.OperationOrProduct)
                .NotNull()
                .WithMessage($"OperationOrProduct is required for {PaTypeDto.LessThanOnePerShift}")
                .SetValidator(operationOrProductValidator);

            RuleFor(x => x.Product)
                .Null()
                .WithMessage($"Product must be null for {PaTypeDto.LessThanOnePerShift}");

            RuleFor(x => x.Products)
                .Null()
                .WithMessage($"Products must be null for {PaTypeDto.LessThanOnePerShift}");
        });
    }
}