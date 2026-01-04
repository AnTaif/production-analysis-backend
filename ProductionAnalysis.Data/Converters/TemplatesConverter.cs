using ProductionAnalysis.Application.Domain.Templates;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Converters;

public static class TemplatesConverter
{
    public static Template ToDomain(this TemplateDbo dbo)
    {
        return new Template(
            dbo.Id,
            dbo.Name,
            dbo.PaTypeId,
            dbo.Version,
            dbo.Indicators.Select(i => i.ToDomain()).ToList());
    }

    public static Indicator ToDomain(this IndicatorDbo dbo)
    {
        return new Indicator(
            dbo.Id,
            dbo.Name,
            dbo.ValueType,
            dbo.InputType,
            dbo.ValueSelector,
            dbo.Formula,
            dbo.IsCumulative,
            dbo.HasSummation);
    }
}