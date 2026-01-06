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
            WorkTime = template.Indicators.FirstOrDefault(i => i.Id == ShiftConstants.WorktimeIndicatorId),
            Plan = template.Indicators.FirstOrDefault(i => i.Id == ShiftConstants.PlanIndicatorId),
            PlanMinutes = template.Indicators.FirstOrDefault(i => i.Id == ShiftConstants.PlanMinutesIndicatorId),
            FactMinutes = template.Indicators.FirstOrDefault(i => i.Id == ShiftConstants.FactMinutesIndicatorId),
            StartTimePlan = template.Indicators.FirstOrDefault(i => i.Id == ShiftConstants.StartTimePlanIndicatorId),
            StartTimeFact = template.Indicators.FirstOrDefault(i => i.Id == ShiftConstants.StartTimeFactIndicatorId),
            EndTimePlan = template.Indicators.FirstOrDefault(i => i.Id == ShiftConstants.EndTimePlanIndicatorId),
            EndTimeFact = template.Indicators.FirstOrDefault(i => i.Id == ShiftConstants.EndTimeFactIndicatorId),
            OperationName = template.Indicators.FirstOrDefault(i => i.Id == ShiftConstants.OperationNameIndicatorId),
            OperationTime = template.Indicators.FirstOrDefault(i => i.Id == ShiftConstants.OperationTimeIndicatorId)
        };
    }
}