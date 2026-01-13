namespace ProductionAnalysis.Client.Models.Forms;

public record FormDto
{
    public int Id { get; init; }
    public PaTypeDto PaType { get; init; }
    public FormStatus Status { get; init; }
    public DateTime CreationDate { get; init; }
    public DateTime UpdateDate { get; init; }
    public DateTime FormDate { get; init; }
    public required FormContextDto Context { get; init; }
    public ICollection<FormRowDto> Rows { get; init; } = new List<FormRowDto>();
    public FormTemplateDto Template { get; init; } = null!;
    public Dictionary<int, object>? TotalValues { get; init; }
}