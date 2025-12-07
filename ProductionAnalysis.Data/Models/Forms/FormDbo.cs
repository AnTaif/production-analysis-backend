namespace ProductionAnalysis.Data.Models.Forms;

public class FormDbo
{
    public int Id { get; set; }

    public int PaTypeId { get; set; }

    public int Status { get; set; }

    public required string Context { get; set; }

    public required string TemplateSnapshot { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime UpdateDate { get; set; }

    public Guid CreatorId { get; set; }

    public Guid LastEditorId { get; set; }
}