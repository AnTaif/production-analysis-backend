using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies;

public class SingleProductWithCycleTimeInitializationStrategy(
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
        return paType == PaType.SingleProductWithCycleTime;
    }
}