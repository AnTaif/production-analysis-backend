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
        var allOperations = await LoadAllOperationsAsync();
        var sortedBreaks = schedules.OrderBy(s => s.StartTime).ToList();

        var context = CreateContext(
            shiftStartTime,
            sortedBreaks,
            template,
            formContext,
            auxiliaryOperations,
            allOperations,
            indicators
        );

        var strategy = strategyFactory.GetStrategy(template.PaType);
        var rows = strategy.Initialize(context);

        cumulativeValueCalculator.FillCumulativeValues(rows, template.Indicators);

        return rows;
    }

    private async Task<Dictionary<int, AuxiliaryOperationDto>> LoadAuxiliaryOperationsAsync()
    {
        var operations = await unitOfWork.Dictionaries.SelectAuxiliaryOperationsAsync();
        return operations.ToDictionary(ao => ao.Id);
    }

    private async Task<ICollection<OperationDto>> LoadAllOperationsAsync()
    {
        return await unitOfWork.Dictionaries.SelectOperationsAsync();
    }

    private static RowInitializationContext CreateContext(
        TimeOnly shiftStartTime,
        List<ShiftScheduleDto> sortedSchedules,
        Template template,
        Dictionary<string, FormContext>? formContext,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        ICollection<OperationDto> allOperations,
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
            AllOperations = allOperations,
            Indicators = initializedIndicators
        };
    }
}