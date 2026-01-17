using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Implementation.Forms.Initialization.Constants;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;

public interface IRetoolingOperationHandler
{
    FormRowData? CreateRetoolingRow(
        TimeOnly startTime,
        short order,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        InitializedIndicators indicators);
}

[RegisterScoped]
public class RetoolingOperationHandler(IFormRowDataFactory formRowDataFactory) : IRetoolingOperationHandler
{
    public FormRowData? CreateRetoolingRow(
        TimeOnly startTime,
        short order,
        Dictionary<int, AuxiliaryOperationDto> auxiliaryOperations,
        InitializedIndicators indicators)
    {
        if (!auxiliaryOperations.TryGetValue(AuxiliaryOperationIds.Retooling, out var retoolingOperation))
            return null;

        var endTime = startTime.Add(retoolingOperation.Duration);

        return formRowDataFactory.CreateBreakRow(
            order,
            indicators.WorkTime,
            startTime,
            endTime,
            retoolingOperation.Name,
            AuxiliaryOperationIds.Retooling,
            null);
    }
}