using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;

public abstract class RowInitializationStrategyBase : IRowInitializationStrategy
{
    public abstract bool CanHandle(PaType paType);

    public abstract Task<ICollection<FormRowData>> InitializeAsync(RowInitializationContext context);

    protected static ShiftScheduleDto? GetNextBreak(IList<ShiftScheduleDto> sortedBreaks, int breakIndex)
    {
        return breakIndex < sortedBreaks.Count ? sortedBreaks[breakIndex] : null;
    }

    protected static ICollection<FormRowData> ProcessRemainingBreaks(
        IList<ShiftScheduleDto> sortedBreaks,
        int breakIndex,
        short startOrder,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        InitializedIndicators indicators,
        IBreakProcessor breakProcessor,
        ProductContext? productContext = null,
        bool isLast = true)
    {
        var remainingBreaks = sortedBreaks.Skip(breakIndex).ToList();
        if (remainingBreaks.Count == 0) return new List<FormRowData>();

        return breakProcessor.ProcessRemainingBreaks(
            remainingBreaks,
            startOrder,
            auxiliaryOperations,
            indicators,
            productContext,
            isLast);
    }
}