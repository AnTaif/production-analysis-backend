using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies;

/// <summary>
///     Стратегия инициализации для одного продукта с пропускной способностью рабочего места
/// </summary>
public class SingleProductWithWorkstationCapacityInitializationStrategy(
    IFormRowDataFactory formRowDataFactory,
    IBreakProcessor breakProcessor,
    IShiftTimeManager shiftTimeManager,
    ICleanupOperationHandler cleanupOperationHandler
)
    : SingleProductInitializationStrategyBase(formRowDataFactory, breakProcessor, shiftTimeManager,
        cleanupOperationHandler)
{
    public override bool CanHandle(PaType paType)
    {
        return paType == PaType.SingleProductWithWorkstationCapacity;
    }
}