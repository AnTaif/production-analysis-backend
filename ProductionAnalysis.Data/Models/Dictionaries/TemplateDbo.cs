using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Data.Models.Dictionaries;

public class TemplateDbo
{
    public int Id { get; set; }

    [MaxLength(255)]
    public required string Name { get; set; }

    public int PaTypeId { get; set; }

    public int Version { get; set; }

    public ICollection<IndicatorDbo> Indicators { get; set; } = new List<IndicatorDbo>();

    public ICollection<TemplateIndicatorDbo> TemplateIndicators { get; set; } = new List<TemplateIndicatorDbo>();
}