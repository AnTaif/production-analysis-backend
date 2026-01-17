using ProductionAnalysis.Client.Models.Dictionaries;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;

public interface ITimeIntervalCalculator
{
    double CalculateTimeUntilBreak(TimeOnly currentTime, ShiftScheduleDto? breakSchedule, TimeSpan remainingWorkTime);
}

[RegisterScoped]
public class TimeIntervalCalculator : ITimeIntervalCalculator
{
    public double CalculateTimeUntilBreak(
        TimeOnly currentTime,
        ShiftScheduleDto? breakSchedule,
        TimeSpan remainingWorkTime
    )
    {
        if (breakSchedule == null)
            return remainingWorkTime.TotalSeconds;

        var timeUntilBreak = TimeHelper.CalculateDurationAcrossMidnight(currentTime, breakSchedule.StartTime);
        return timeUntilBreak.TotalSeconds;
    }
}