namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;

public interface IWorkTimeTracker
{
    TimeSpan ElapsedWorkTime { get; }
    TimeSpan RemainingWorkTime { get; }
    bool IsComplete { get; }
    bool CanAddInterval(TimeSpan duration);
    TimeSpan GetAdjustedDuration(TimeSpan requestedDuration);
    void Add(TimeSpan duration);
}

[RegisterScoped]
public class WorkTimeTracker(IShiftTimeManager shiftTimeManager) : IWorkTimeTracker
{
    private TimeSpan elapsedWorkTime = TimeSpan.Zero;
    private readonly TimeSpan totalWorkTime = shiftTimeManager.GetTotalWorkTime();

    public TimeSpan ElapsedWorkTime => elapsedWorkTime;

    public TimeSpan RemainingWorkTime => shiftTimeManager.GetRemainingWorkTime(elapsedWorkTime);

    public bool IsComplete => shiftTimeManager.IsWorkTimeComplete(elapsedWorkTime, totalWorkTime);

    public bool CanAddInterval(TimeSpan duration)
    {
        return shiftTimeManager.CanAddWorkInterval(elapsedWorkTime, duration);
    }

    public TimeSpan GetAdjustedDuration(TimeSpan requestedDuration)
    {
        if (CanAddInterval(requestedDuration))
            return requestedDuration;

        var remaining = RemainingWorkTime;
        return remaining > requestedDuration ? requestedDuration : remaining;
    }

    public void Add(TimeSpan duration)
    {
        if (!CanAddInterval(duration))
        {
            var adjusted = GetAdjustedDuration(duration);
            elapsedWorkTime = elapsedWorkTime.Add(adjusted);
        }
        else
        {
            elapsedWorkTime = elapsedWorkTime.Add(duration);
        }
    }
}