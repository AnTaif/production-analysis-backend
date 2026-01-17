using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;

public abstract class RowInitializationStrategyBase(
    ICleanupOperationHandler cleanupHandler,
    IFormRowEndTimeExtractor endTimeExtractor,
    IBreakProcessor breakProcessor
) : IRowInitializationStrategy
{
    public abstract bool CanHandle(PaType paType);

    public ICollection<FormRowData> Initialize(RowInitializationContext context)
    {
        var filteredContext = CreateFilteredContext(context);
        var rows = InitializeRows(filteredContext);
        return AppendCleanupOperation(rows, context);
    }

    protected abstract ICollection<FormRowData> InitializeRows(RowInitializationContext context);

    protected static ShiftScheduleDto? GetNextBreak(IList<ShiftScheduleDto> sortedBreaks, int breakIndex)
    {
        return breakIndex < sortedBreaks.Count ? sortedBreaks[breakIndex] : null;
    }

    protected bool IsCleanupOperation(int auxiliaryOperationId)
    {
        return cleanupHandler.IsCleanupOperation(auxiliaryOperationId);
    }

    protected ICollection<ShiftScheduleDto> FilterOutCleanup(ICollection<ShiftScheduleDto> schedules)
    {
        return cleanupHandler.FilterOutCleanup(schedules);
    }

    protected ICollection<FormRowData> FilterOutCleanup(ICollection<FormRowData> rows)
    {
        return cleanupHandler.FilterOutCleanup(rows);
    }

    protected ICollection<FormRowData> ProcessRemainingBreaks(
        IList<ShiftScheduleDto> sortedBreaks,
        int breakIndex,
        short startOrder,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        InitializedIndicators indicators,
        ProductContext? productContext = null,
        bool isLast = true)
    {
        var remainingBreaks = FilterOutCleanup(
            sortedBreaks.Skip(breakIndex).ToList());

        if (remainingBreaks.Count == 0)
            return new List<FormRowData>();

        return breakProcessor.ProcessRemainingBreaks(
            remainingBreaks,
            startOrder,
            auxiliaryOperations,
            indicators,
            productContext,
            isLast);
    }

    private RowInitializationContext CreateFilteredContext(RowInitializationContext originalContext)
    {
        var filteredSchedules = cleanupHandler.FilterOutCleanup(originalContext.SortedSchedules);

        return new RowInitializationContext
        {
            ShiftStartTime = originalContext.ShiftStartTime,
            SortedSchedules = filteredSchedules.ToList(),
            Template = originalContext.Template,
            FormContext = originalContext.FormContext,
            AuxiliaryOperations = originalContext.AuxiliaryOperations,
            AllOperations = originalContext.AllOperations,
            Indicators = originalContext.Indicators
        };
    }

    private List<FormRowData> AppendCleanupOperation(
        ICollection<FormRowData> rows,
        RowInitializationContext context)
    {
        var rowsList = rows.ToList();
        var endTime = endTimeExtractor.ExtractEndTime(rowsList);
        var nextOrder = CalculateNextOrder(rowsList);

        var cleanupRow = cleanupHandler.CreateCleanupRow(
            endTime,
            nextOrder,
            context.AuxiliaryOperations,
            context.Indicators);

        if (cleanupRow != null)
        {
            rowsList.Add(cleanupRow);
        }

        return rowsList;
    }

    private static short CalculateNextOrder(List<FormRowData> rows)
    {
        if (rows.Count == 0)
            return 1;

        var maxOrder = rows[0].Order;
        for (var i = 1; i < rows.Count; i++)
        {
            if (rows[i].Order > maxOrder)
                maxOrder = rows[i].Order;
        }

        return (short)(maxOrder + 1);
    }
}