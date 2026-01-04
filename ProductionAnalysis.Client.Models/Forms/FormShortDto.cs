namespace ProductionAnalysis.Client.Models.Forms;

public record FormShortDto
{
    public int Id { get; init; }
    public int PaTypeId { get; init; }
    public FormStatus Status { get; init; }
    public DateTime CreationDate { get; init; }
    public DateTime UpdateDate { get; init; }
    public int DepartmentId { get; init; }
}