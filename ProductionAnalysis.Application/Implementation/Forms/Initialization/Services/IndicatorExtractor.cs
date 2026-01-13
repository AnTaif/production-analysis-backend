using ProductionAnalysis.Application.Domain.Templates;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;

public interface IIndicatorExtractor
{
    InitializedIndicators Extract(Template template);
}

[RegisterScoped]
public class IndicatorExtractor : IIndicatorExtractor
{
    public InitializedIndicators Extract(Template template)
    {
        return new InitializedIndicators
        {
            WorkTime = template.Indicators.FirstOrDefault(i => i.Id == IndicatorConstants.WorktimeId),
            Plan = template.Indicators.FirstOrDefault(i => i.Id == IndicatorConstants.PlanId),
            PlanMinutes = template.Indicators.FirstOrDefault(i => i.Id == IndicatorConstants.PlanMinutesId),
            FactMinutes = template.Indicators.FirstOrDefault(i => i.Id == IndicatorConstants.FactMinutesId),
            StartTimePlan = template.Indicators.FirstOrDefault(i => i.Id == IndicatorConstants.StartTimePlanId),
            StartTimeFact = template.Indicators.FirstOrDefault(i => i.Id == IndicatorConstants.StartTimeFactId),
            EndTimePlan = template.Indicators.FirstOrDefault(i => i.Id == IndicatorConstants.EndTimePlanId),
            EndTimeFact = template.Indicators.FirstOrDefault(i => i.Id == IndicatorConstants.EndTimeFactId),
            OperationName = template.Indicators.FirstOrDefault(i => i.Id == IndicatorConstants.OperationNameId),
            OperationTime = template.Indicators.FirstOrDefault(i => i.Id == IndicatorConstants.OperationTimeId)
        };
    }
}