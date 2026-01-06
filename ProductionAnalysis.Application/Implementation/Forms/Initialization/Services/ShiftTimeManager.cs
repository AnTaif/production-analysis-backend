namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;

public interface IShiftTimeManager
{
    TimeSpan GetTotalWorkTime();
    TimeSpan CalculateWorkIntervalDuration(TimeSpan remainingWorkTime);
    bool IsWorkTimeComplete(TimeSpan elapsedWorkTime, TimeSpan totalWorkTime);
}

[RegisterScoped]
public class ShiftTimeManager : IShiftTimeManager
{
    public TimeSpan GetTotalWorkTime()
    {
        return TimeSpan.FromHours(ShiftConstants.ShiftDurationHours);
    }

    public TimeSpan CalculateWorkIntervalDuration(TimeSpan remainingWorkTime)
    {
        return remainingWorkTime >= TimeSpan.FromHours(1)
            ? TimeSpan.FromHours(1)
            : remainingWorkTime;
    }

    public bool IsWorkTimeComplete(TimeSpan elapsedWorkTime, TimeSpan totalWorkTime)
    {
        return elapsedWorkTime >= totalWorkTime;
    }
}