namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;

public static class TimeHelper
{
    public static TimeOnly AdjustForMidnight(TimeOnly time, TimeSpan duration)
    {
        var endTime = time.Add(duration);

        if (endTime >= time)
            return endTime;

        var timeUntilMidnight = TimeSpan.FromDays(1) - TimeSpan.FromTicks(time.Ticks);
        var minutesUntilMidnight = (int)timeUntilMidnight.TotalMinutes;
        var adjustedTime = time.AddMinutes(minutesUntilMidnight);

        return adjustedTime < time ? new TimeOnly(23, 59) : adjustedTime;
    }

    public static TimeSpan CalculateDurationAcrossMidnight(TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime >= startTime)
            return endTime - startTime;

        var timeUntilMidnight = TimeSpan.FromDays(1) - TimeSpan.FromTicks(startTime.Ticks);
        return timeUntilMidnight + TimeSpan.FromTicks(endTime.Ticks);
    }

    public static TimeOnly GetMaxEndTime(TimeOnly currentTime, TimeSpan remainingWorkTime)
    {
        var maxEndTime = currentTime.Add(remainingWorkTime);

        if (maxEndTime >= currentTime)
            return maxEndTime;

        return AdjustForMidnight(currentTime, remainingWorkTime);
    }
}