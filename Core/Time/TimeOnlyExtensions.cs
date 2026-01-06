namespace Core.Time;

public static class TimeOnlyExtensions
{
    public static int TotalMinutes(this TimeOnly time)
    {
        return time.Hour * 60 + time.Minute;
    }
}