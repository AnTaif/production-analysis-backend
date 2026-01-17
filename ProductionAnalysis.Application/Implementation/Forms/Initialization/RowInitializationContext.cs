using ProductionAnalysis.Application.Domain.Forms.Context;
using ProductionAnalysis.Application.Domain.Templates;
using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization;

public class RowInitializationContext
{
    public TimeOnly ShiftStartTime { get; init; }
    public IList<ShiftScheduleDto> SortedSchedules { get; init; } = new List<ShiftScheduleDto>();
    public Template Template { get; init; } = null!;
    public required Dictionary<string, FormContext> FormContext { get; init; }
    public Dictionary<int, AuxiliaryOperationDto> AuxiliaryOperations { get; init; } = new();
    public ICollection<OperationDto> AllOperations { get; init; } = new List<OperationDto>();
    public InitializedIndicators Indicators { get; init; } = null!;
}