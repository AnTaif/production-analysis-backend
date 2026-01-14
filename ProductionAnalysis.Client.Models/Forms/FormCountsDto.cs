namespace ProductionAnalysis.Client.Models.Forms;

public record FormCountsDto
{
    public int Total { get; init; }
    public int InProgress { get; init; }
    public int Completed { get; init; }
}