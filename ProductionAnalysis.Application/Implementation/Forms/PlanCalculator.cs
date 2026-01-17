using ProductionAnalysis.Application.Domain.Forms.Context;

namespace ProductionAnalysis.Application.Implementation.Forms;

public interface IPlanCalculator
{
    int Calculate(TimeOnly startTime, TimeOnly endTime, ProductContext productContext);
}

[RegisterScoped]
public class PlanCalculator : IPlanCalculator
{
    public int Calculate(TimeOnly startTime, TimeOnly endTime, ProductContext productContext)
    {
        var workDuration = endTime - startTime;

        if (workDuration <= TimeSpan.MinValue)
        {
            return 0;
        }

        if (productContext.WorkstationCapacity is > 0)
        {
            var plan = workDuration.TotalHours * productContext.WorkstationCapacity.Value;
            return (int)Math.Round(plan);
        }

        if (productContext.CycleTime is > 0)
        {
            var plan = workDuration.TotalSeconds / productContext.CycleTime.Value;
            return (int)plan;
        }

        throw new Exception("Invalid context");
    }
}