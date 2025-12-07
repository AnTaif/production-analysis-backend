namespace ProductionAnalysis.Application.Domain.Forms;

public class Form
{
    public Guid Id { get; set; }
    public int PaTypeId { get; set; }
    public FormStatus Status { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime UpdateDate { get; set; }
}