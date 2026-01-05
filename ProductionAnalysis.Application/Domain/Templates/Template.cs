using ProductionAnalysis.Application.Domain.Forms;

namespace ProductionAnalysis.Application.Domain.Templates;

public class Template
{
    public Template(
        int id,
        string name,
        PaType paType,
        int version,
        ICollection<Indicator> indicators)
    {
        Id = id;
        Name = name;
        PaType = paType;
        Version = version;
        Indicators = indicators;
    }

    public int Id { get; }
    public string Name { get; }
    public PaType PaType { get; }
    public int Version { get; }
    public ICollection<Indicator> Indicators { get; }
}