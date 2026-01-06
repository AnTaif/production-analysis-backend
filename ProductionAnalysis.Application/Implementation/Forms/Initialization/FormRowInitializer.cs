using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Domain.Templates;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;
using ProductionAnalysis.Application.Repositories;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization;

public interface IFormRowInitializer
{
    Task<ICollection<FormRowData>> InitializeRowsForShiftAsync(
        TimeOnly shiftStartTime,
        ICollection<ShiftScheduleDto> schedules,
        Template template,
        Dictionary<string, FormContext>? formContext = null);
}

[RegisterScoped]
public class FormRowInitializer(
    IPaUnitOfWork unitOfWork,
    IRowInitializationStrategyFactory strategyFactory,
    IIndicatorExtractor indicatorExtractor,
    ICumulativeValueCalculator cumulativeValueCalculator
)
    : IFormRowInitializer
{
    public async Task<ICollection<FormRowData>> InitializeRowsForShiftAsync(
        TimeOnly shiftStartTime,
        ICollection<ShiftScheduleDto> schedules,
        Template template,
        Dictionary<string, FormContext>? formContext = null)
    {
        var indicators = indicatorExtractor.Extract(template);
        var auxiliaryOperations = await LoadAuxiliaryOperationsAsync();
        var sortedBreaks = schedules.OrderBy(s => s.StartTime).ToList();

        var context = CreateContext(
            shiftStartTime,
            sortedBreaks,
            template,
            formContext,
            auxiliaryOperations,
            indicators
        );

        var strategy = strategyFactory.GetStrategy(template.PaType);
        var rows = await strategy.InitializeAsync(context);

        cumulativeValueCalculator.FillCumulativeValues(rows, template.Indicators);

        return rows;
    }

    private async Task<Dictionary<int, AuxiliaryOperationDto>> LoadAuxiliaryOperationsAsync()
    {
        var operations = await unitOfWork.Dictionaries.SelectAuxiliaryOperationsAsync();
        return operations.ToDictionary(ao => ao.Id);
    }

    private static RowInitializationContext CreateContext(
        TimeOnly shiftStartTime,
        List<ShiftScheduleDto> sortedSchedules,
        Template template,
        Dictionary<string, FormContext>? formContext,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        InitializedIndicators initializedIndicators
    )
    {
        return new RowInitializationContext
        {
            ShiftStartTime = shiftStartTime,
            SortedSchedules = sortedSchedules,
            Template = template,
            FormContext = formContext ?? new Dictionary<string, FormContext>(),
            AuxiliaryOperations = auxiliaryOperations,
            Indicators = initializedIndicators
        };
    }
}