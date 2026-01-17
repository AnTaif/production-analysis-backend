namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;

public interface IWorkTimeTracker
{
    TimeOnly CurrentTime { get; }
    TimeSpan ElapsedWorkTime { get; }
    TimeSpan RemainingWorkTime { get; }
    TimeSpan TotalWorkTime { get; }
    bool IsComplete { get; }
    TimeSpan GetNextWorkIntervalDuration();
    TimeSpan GetAdjustedDuration(TimeSpan requestedDuration);
    TimeSpan AdvanceWorktime(TimeSpan duration);
    void AdvanceTime(TimeSpan duration);
}

public class WorkTimeTracker(TimeOnly shiftStartTime) : IWorkTimeTracker
{
    private TimeSpan elapsedWorkTime = TimeSpan.Zero;
    private TimeOnly currentTime = shiftStartTime;

    public TimeOnly CurrentTime => currentTime;
    public TimeSpan ElapsedWorkTime => elapsedWorkTime;
    public TimeSpan TotalWorkTime { get; } = TimeSpan.FromHours(ShiftConstants.ShiftDurationHours);

    public TimeSpan RemainingWorkTime
    {
        get
        {
            var remaining = TotalWorkTime - elapsedWorkTime;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }

    public bool IsComplete => elapsedWorkTime >= TotalWorkTime;

    public TimeSpan GetNextWorkIntervalDuration()
    {
        var remaining = RemainingWorkTime;
        return remaining >= TimeSpan.FromHours(1)
            ? TimeSpan.FromHours(1)
            : remaining;
    }

    public TimeSpan GetAdjustedDuration(TimeSpan requestedDuration)
    {
        if (CanAddInterval(requestedDuration))
            return requestedDuration;

        return RemainingWorkTime;
    }

    public TimeSpan AdvanceWorktime(TimeSpan duration)
    {
        var adjusted = GetAdjustedDuration(duration);
        elapsedWorkTime = elapsedWorkTime.Add(adjusted);
        currentTime = currentTime.Add(adjusted);
        return adjusted;
    }

    public void AdvanceTime(TimeSpan duration)
    {
        currentTime = currentTime.Add(duration);
    }

    private bool CanAddInterval(TimeSpan duration)
    {
        return elapsedWorkTime + duration <= TotalWorkTime;
    }
}