using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Application.Domain.Templates;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Converters;

public static class TemplatesConverter
{
    public static Template ToDomain(this TemplateDbo dbo)
    {
        var paType = (PaType)dbo.PaTypeId;

        var indicators = dbo.TemplateIndicators
            .OrderBy(ti => ti.Order)
            .Select(ti => ti.Indicator.ToDomain(ti.Order))
            .ToList();

        return new Template(
            dbo.Id,
            dbo.Name,
            paType,
            dbo.Version,
            indicators);
    }

    public static Indicator ToDomain(this IndicatorDbo dbo, int order)
    {
        return new Indicator(
            dbo.Id,
            dbo.Name,
            dbo.ValueType,
            dbo.InputType,
            dbo.ValueSelector,
            dbo.Formula,
            dbo.IsCumulative,
            dbo.HasSummation,
            order);
    }
}