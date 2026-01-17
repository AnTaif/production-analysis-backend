using ProductionAnalysis.Application.Domain.Forms;

namespace ProductionAnalysis.Application.Implementation.Forms.Initialization.Services;

public interface IFormRowEndTimeExtractor
{
    TimeOnly ExtractEndTime(FormRowData row);
    TimeOnly ExtractEndTime(ICollection<FormRowData> rows);
}

[RegisterScoped]
public class FormRowEndTimeExtractor : IFormRowEndTimeExtractor
{
    public TimeOnly ExtractEndTime(FormRowData row)
    {
        foreach (var value in row.Values)
        {
            if (value.Value is TimeOnly timeOnly)
            {
                return timeOnly;
            }

            if (value.Value is string timeString)
            {
                var endTime = TryParseEndTimeFromString(timeString);
                if (endTime.HasValue)
                {
                    return endTime.Value;
                }
            }
        }

        return new TimeOnly(23, 59);
    }

    public TimeOnly ExtractEndTime(ICollection<FormRowData> rows)
    {
        if (rows.Count == 0)
            return new TimeOnly(0, 0);

        return ExtractEndTime(rows.Last());
    }

    private static TimeOnly? TryParseEndTimeFromString(string timeString)
    {
        var parts = timeString.Split('-');
        if (parts.Length < 2)
            return null;

        var endTimePart = parts[1].Split(' ')[0].Trim();
        return TimeOnly.TryParse(endTimePart, out var endTime) ? endTime : null;
    }
}