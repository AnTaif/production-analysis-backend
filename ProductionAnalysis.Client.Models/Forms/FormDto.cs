namespace ProductionAnalysis.Client.Models.Forms;

public record FormDto
{
    public int Id { get; init; }
    public int PaTypeId { get; init; }
    public FormStatus Status { get; init; }
    public DateTime CreationDate { get; init; }
    public DateTime UpdateDate { get; init; }
    public Dictionary<string, object> Context { get; init; } = new();
    public ICollection<FormRowDto> Rows { get; init; } = new List<FormRowDto>();
    public FormTemplateDto Template { get; init; } = null!;
}