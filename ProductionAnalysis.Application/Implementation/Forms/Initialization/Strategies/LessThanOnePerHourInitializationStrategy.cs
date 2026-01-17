using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies.Common;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Strategies;

public class LessThanOnePerHourInitializationStrategy(
    IFormRowDataFactory formRowDataFactory,
    IBreakProcessor breakProcessor,
    IShiftTimeManager shiftTimeManager,
    IOperationService operationService,
    ICleanupOperationHandler cleanupHandler
)
    : OperationOrProductInitializationStrategyBase(operationService, cleanupHandler), IRowInitializationStrategy
{
    public override bool CanHandle(PaType paType)
    {
        return paType == PaType.LessThanOnePerHour;
    }

    protected override async Task<ICollection<FormRowData>> InitializeRowsAsync(RowInitializationContext context)
    {
        var operationContext =
            context.FormContext.Require<OperationOrProductContext>(FormContextAccessor.OperationOrProductContextKey);
        var relatedOperations = await GetRelatedOperationsAsync(operationContext);

        var totalWorkTime = shiftTimeManager.GetTotalWorkTime();
        var cycleDuration = OperationService.CalculateCycleDuration(relatedOperations);

        var rows = new List<FormRowData>();
        var currentTime = context.ShiftStartTime;
        var elapsedWorkTime = TimeSpan.Zero;
        var breakIndex = 0;
        short order = 1;

        while (!shiftTimeManager.IsWorkTimeComplete(elapsedWorkTime, totalWorkTime))
        {
            var nextBreak = GetNextBreak(context.SortedSchedules, breakIndex);
            var remainingWorkTime = totalWorkTime - elapsedWorkTime;

            var timeUntilBreak = nextBreak != null
                ? (nextBreak.StartTime - currentTime).TotalSeconds
                : remainingWorkTime.TotalSeconds;

            if (ShouldProcessBreak(nextBreak, timeUntilBreak, cycleDuration))
            {
                var breakMetaInfo = context.AuxiliaryOperations[nextBreak!.AuxiliaryOperationId];
                var breakEndTime = nextBreak.StartTime.Add(breakMetaInfo.Duration);

                var breakRow = formRowDataFactory.CreateBreakRow(
                    order++,
                    context.Indicators.WorkTime,
                    nextBreak.StartTime,
                    breakEndTime,
                    breakMetaInfo.Name,
                    nextBreak.AuxiliaryOperationId,
                    null);

                rows.Add(breakRow);
                currentTime = breakEndTime;
                elapsedWorkTime = elapsedWorkTime.Add(breakMetaInfo.Duration);
                breakIndex++;
            }
            else if (remainingWorkTime.TotalSeconds >= cycleDuration)
            {
                var cycleEndTime = currentTime.Add(TimeSpan.FromSeconds(cycleDuration));

                var cycleRows = formRowDataFactory.CreateOperationCycleRows(
                    ref order,
                    context.Indicators.WorkTime!,
                    context.Indicators.Plan,
                    context.Indicators.OperationName,
                    context.Indicators.OperationTime,
                    currentTime,
                    cycleEndTime,
                    relatedOperations);

                rows.AddRange(cycleRows);
                currentTime = cycleEndTime;
                elapsedWorkTime = elapsedWorkTime.Add(TimeSpan.FromSeconds(cycleDuration));
            }
            else
            {
                var cycleEndTime = currentTime.Add(remainingWorkTime);

                var cycleRows = formRowDataFactory.CreateOperationCycleRows(
                    ref order,
                    context.Indicators.WorkTime!,
                    context.Indicators.Plan,
                    context.Indicators.OperationName,
                    context.Indicators.OperationTime,
                    currentTime,
                    cycleEndTime,
                    relatedOperations);

                rows.AddRange(cycleRows);
                break;
            }
        }

        var remainingBreaks = FilterOutCleanup(
            context.SortedSchedules.Skip(breakIndex).ToList());

        if (remainingBreaks.Count > 0)
        {
            var remainingBreakRows = breakProcessor.ProcessRemainingBreaks(
                remainingBreaks,
                order,
                context.AuxiliaryOperations,
                context.Indicators,
                null);

            rows.AddRange(remainingBreakRows);
        }

        return rows;
    }

    private bool ShouldProcessBreak(ShiftScheduleDto? nextBreak, double timeUntilBreak, double cycleDuration)
    {
        if (nextBreak == null)
            return false;

        return timeUntilBreak > 0 && timeUntilBreak < cycleDuration;
    }
}