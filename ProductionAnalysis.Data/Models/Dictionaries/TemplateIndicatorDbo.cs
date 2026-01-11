namespace ProductionAnalysis.Data.Models.Dictionaries;

public class TemplateIndicatorDbo
{
    public int TemplateId { get; set; }
    public TemplateDbo Template { get; set; } = null!;

    public int IndicatorId { get; set; }
    public IndicatorDbo Indicator { get; set; } = null!;

    public int Order { get; set; }
}