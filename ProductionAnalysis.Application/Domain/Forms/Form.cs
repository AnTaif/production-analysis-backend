namespace ProductionAnalysis.Application.Domain.Forms;

public class Form
{
    public int Id { get; set; }
    public int PaTypeId { get; set; }
    public FormStatus Status { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public Dictionary<string, FormContextBase> Context { get; set; } = new();
    public string TemplateSnapshot { get; set; } = string.Empty;
    public ICollection<FormRow> Rows { get; set; } = new List<FormRow>();
    public Guid CreatorId { get; set; }
    public int ShiftId { get; set; }
    public int DepartmentId { get; set; }
}