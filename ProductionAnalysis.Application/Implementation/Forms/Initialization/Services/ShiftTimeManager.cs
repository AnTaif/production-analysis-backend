namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;

public interface IShiftTimeManager
{
    TimeSpan GetTotalWorkTime();
    TimeSpan CalculateWorkIntervalDuration(TimeSpan remainingWorkTime);
    bool IsWorkTimeComplete(TimeSpan elapsedWorkTime, TimeSpan totalWorkTime);
    bool CanAddWorkInterval(TimeSpan elapsedWorkTime, TimeSpan intervalDuration);
    TimeSpan GetRemainingWorkTime(TimeSpan elapsedWorkTime);
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

    public bool CanAddWorkInterval(TimeSpan elapsedWorkTime, TimeSpan intervalDuration)
    {
        return elapsedWorkTime + intervalDuration <= GetTotalWorkTime();
    }

    public TimeSpan GetRemainingWorkTime(TimeSpan elapsedWorkTime)
    {
        var totalWorkTime = GetTotalWorkTime();
        var remaining = totalWorkTime - elapsedWorkTime;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }
}