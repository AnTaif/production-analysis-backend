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

        if (workDuration <= TimeSpan.MinValue || productContext.CycleTime <= 0)
        {
            return 0;
        }

        var plan = workDuration.TotalSeconds / productContext.CycleTime;

        return (int)plan;
    }
}