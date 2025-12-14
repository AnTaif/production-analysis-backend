namespace ProductionAnalysis.Application.Domain.Templates;

public class Template
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int PaTypeId { get; set; }
    public int Version { get; set; }
    public ICollection<Indicator> Indicators { get; set; } = new List<Indicator>();
}