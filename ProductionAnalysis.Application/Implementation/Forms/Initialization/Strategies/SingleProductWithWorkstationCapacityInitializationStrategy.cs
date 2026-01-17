using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies;

public class SingleProductWithWorkstationCapacityInitializationStrategy(
    IFormRowDataFactory formRowDataFactory,
    IBreakProcessor breakProcessor,
    IShiftTimeManager shiftTimeManager,
    ICleanupOperationHandler cleanupOperationHandler,
    IFormRowEndTimeExtractor endTimeExtractor
)
    : SingleProductInitializationStrategyBase(formRowDataFactory, breakProcessor, shiftTimeManager,
        cleanupOperationHandler, endTimeExtractor)
{
    public override bool CanHandle(PaType paType)
    {
        return paType == PaType.SingleProductWithWorkstationCapacity;
    }
}