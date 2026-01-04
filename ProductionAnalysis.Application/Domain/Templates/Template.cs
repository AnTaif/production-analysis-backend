namespace ProductionAnalysis.Application.Domain.Templates;

public class Template
{
    public Template(
        int id,
        string name,
        int paTypeId,
        int version,
        ICollection<Indicator> indicators)
    {
        Id = id;
        Name = name;
        PaTypeId = paTypeId;
        Version = version;
        Indicators = indicators;
    }

    public int Id { get; }
    public string Name { get; }
    public int PaTypeId { get; }
    public int Version { get; }
    public ICollection<Indicator> Indicators { get; }
}