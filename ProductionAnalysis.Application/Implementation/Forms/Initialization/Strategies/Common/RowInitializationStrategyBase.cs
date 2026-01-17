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
        IWorkTimeTracker workTimeTracker,
        ProductContext? productContext = null,
        bool isLast = true)
    {
        var remainingBreaks = FilterOutCleanup(
            sortedBreaks.Skip(breakIndex).ToList());

        if (remainingBreaks.Count == 0)
            return new List<FormRowData>();

        var rows = breakProcessor.ProcessRemainingBreaks(
            remainingBreaks,
            startOrder,
            auxiliaryOperations,
            indicators,
            productContext,
            isLast);

        if (rows.Count > 0)
        {
            var lastBreak = remainingBreaks.Last();
            var lastBreakMetaInfo = auxiliaryOperations[lastBreak.AuxiliaryOperationId];
            var breakDuration = lastBreakMetaInfo.Duration;
            workTimeTracker.AdvanceTime(breakDuration);
        }

        return rows;
    }

    protected BreakProcessingResult ProcessBreakWithTracking(
        ShiftScheduleDto breakSchedule,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        InitializedIndicators indicators,
        ProductContext? productContext,
        IWorkTimeTracker workTimeTracker,
        ref short order,
        ref TimeOnly currentTime,
        bool isFirst = false)
    {
        currentTime = workTimeTracker.CurrentTime;
        var elapsedWorkTimeBeforeBreak = workTimeTracker.ElapsedWorkTime;
        var elapsedWorkTimeForBreak = elapsedWorkTimeBeforeBreak;

        var breakResult = breakProcessor.ProcessBreak(
            breakSchedule,
            auxiliaryOperations,
            indicators,
            productContext,
            ref order,
            ref currentTime,
            ref elapsedWorkTimeForBreak,
            isFirst);

        var workTimeUsed = elapsedWorkTimeForBreak - elapsedWorkTimeBeforeBreak;
        if (workTimeUsed > TimeSpan.Zero)
        {
            workTimeTracker.AdvanceWorktime(workTimeUsed);
        }

        var breakMetaInfo = auxiliaryOperations[breakSchedule.AuxiliaryOperationId];
        var breakDuration = breakMetaInfo.Duration;
        workTimeTracker.AdvanceTime(breakDuration);

        return breakResult;
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
            Indicators = originalContext.Indicators,
            WorkTimeTracker = originalContext.WorkTimeTracker
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