using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;

public abstract class RowInitializationStrategyBase(
    ICleanupOperationHandler cleanupHandler
) : IRowInitializationStrategy
{
    public abstract bool CanHandle(PaType paType);

    public async Task<ICollection<FormRowData>> InitializeAsync(RowInitializationContext context)
    {
        var filteredContext = CreateFilteredContext(context);
        var rows = await InitializeRowsAsync(filteredContext);
        return AppendCleanupOperation(rows, context);
    }

    protected abstract Task<ICollection<FormRowData>> InitializeRowsAsync(RowInitializationContext context);

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
            Indicators = originalContext.Indicators
        };
    }

    private List<FormRowData> AppendCleanupOperation(
        ICollection<FormRowData> rows,
        RowInitializationContext context)
    {
        var rowsList = rows.ToList();
        var endTime = CalculateEndTime(rowsList);
        var nextOrder = GetNextOrder(rowsList);

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

    private static TimeOnly CalculateEndTime(List<FormRowData> rows)
    {
        if (rows.Count == 0)
            return new TimeOnly(0, 0);

        var lastRow = rows.Last();

        foreach (var value in lastRow.Values)
        {
            if (value.Value is TimeOnly timeOnly)
            {
                return timeOnly;
            }

            if (value.Value is string timeString)
            {
                var parts = timeString.Split('-');
                if (parts.Length >= 2)
                {
                    var endTimePart = parts[1].Split(' ')[0].Trim();
                    if (TimeOnly.TryParse(endTimePart, out var endTime))
                    {
                        return endTime;
                    }
                }
            }
        }

        return new TimeOnly(23, 59);
    }

    private static short GetNextOrder(List<FormRowData> rows)
    {
        return (short)(rows.Count > 0 ? rows.Max(r => r.Order) + 1 : 1);
    }
}