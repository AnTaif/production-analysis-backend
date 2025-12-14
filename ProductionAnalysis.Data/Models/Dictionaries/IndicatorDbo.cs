using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Data.Models.Dictionaries;

public class IndicatorDbo
{
    public int Id { get; set; }

    [MaxLength(255)] public required string Name { get; set; }

    [MaxLength(50)] public required string ValueType { get; set; }

    // TODO: add InputType

    [MaxLength(100)] public required string ValueSelector { get; set; } // TODO: null

    [MaxLength(255)] public string? Formula { get; set; }

    public bool IsCumulative { get; set; }

    public bool HasSummation { get; set; }

    public ICollection<TemplateDbo> Templates { get; set; } = new List<TemplateDbo>();
}