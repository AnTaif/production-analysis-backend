using ProductionAnalysis.Application.Domain.Templates;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Converters;

public static class TemplatesConverter
{
    public static Template ToDomain(this TemplateDbo dbo)
    {
        return new Template
        {
            Id = dbo.Id,
            Name = dbo.Name,
            PaTypeId = dbo.PaTypeId,
            Version = dbo.Version,
            Indicators = dbo.Indicators.Select(i => i.ToDomain()).ToList()
        };
    }

    public static Indicator ToDomain(this IndicatorDbo dbo)
    {
        return new Indicator
        {
            Id = dbo.Id,
            Name = dbo.Name,
            ValueType = dbo.ValueType,
            InputType = dbo.InputType,
            ValueSelector = dbo.ValueSelector,
            Formula = dbo.Formula,
            IsCumulative = dbo.IsCumulative,
            HasSummation = dbo.HasSummation
        };
    }
}