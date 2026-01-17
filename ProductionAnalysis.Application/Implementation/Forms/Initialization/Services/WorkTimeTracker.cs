namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;

public interface IWorkTimeTracker
{
    TimeSpan ElapsedWorkTime { get; }
    TimeSpan RemainingWorkTime { get; }
    TimeSpan TotalWorkTime { get; }
    bool IsComplete { get; }
    TimeSpan GetNextWorkIntervalDuration();
    TimeSpan GetAdjustedDuration(TimeSpan requestedDuration);
    TimeSpan AddAndGetActual(TimeSpan duration);
    void Add(TimeSpan duration);
}

public class WorkTimeTracker : IWorkTimeTracker
{
    private TimeSpan elapsedWorkTime = TimeSpan.Zero;

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

    public TimeSpan AddAndGetActual(TimeSpan duration)
    {
        var adjusted = GetAdjustedDuration(duration);
        elapsedWorkTime = elapsedWorkTime.Add(adjusted);
        return adjusted;
    }

    public void Add(TimeSpan duration)
    {
        var adjusted = GetAdjustedDuration(duration);
        elapsedWorkTime = elapsedWorkTime.Add(adjusted);
    }

    private bool CanAddInterval(TimeSpan duration)
    {
        return elapsedWorkTime + duration <= TotalWorkTime;
    }
}