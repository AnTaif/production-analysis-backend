using ProductionAnalysis.Application.Domain.Templates;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization;

/// <summary>
///     Извлеченные индикаторы из шаблона
/// </summary>
public record InitializedIndicators
{
    public Indicator? WorkTime { get; init; }
    public Indicator? Plan { get; init; }
    public Indicator? PlanMinutes { get; init; }
    public Indicator? FactMinutes { get; init; }
    public Indicator? StartTimePlan { get; init; }
    public Indicator? StartTimeFact { get; init; }
    public Indicator? EndTimePlan { get; init; }
    public Indicator? EndTimeFact { get; init; }
    public Indicator? OperationName { get; init; }
    public Indicator? OperationTime { get; init; }
}