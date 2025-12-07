namespace ProductionAnalysis.Application.Domain.Forms;

public class CreateForm
{
    public int PaTypeId { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
    public Guid CreatorId { get; set; }
}