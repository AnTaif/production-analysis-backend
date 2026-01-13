namespace ProductionAnalysis.Client.Models.Forms;

public record UpdateFormRowResponse
{
    public ICollection<FormRowDto> Rows { get; init; } = new List<FormRowDto>();
    public Dictionary<int, object> Totals { get; init; } = new Dictionary<int, object>();
}