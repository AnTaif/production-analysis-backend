namespace ProductionAnalysis.Application.Domain.Forms;

public class Form
{
    public int Id { get; set; }
    public int PaTypeId { get; set; }
    public FormStatus Status { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
    public string TemplateSnapshot { get; set; } = string.Empty;
}