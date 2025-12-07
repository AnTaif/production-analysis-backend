using System.Text.Json;
using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Data.Models.Forms;

namespace ProductionAnalysis.Data.Converters;

public static class FormsConverter
{
    public static Form ToDomain(this FormDbo dbo)
    {
        var context = JsonSerializer.Deserialize<Dictionary<string, object>>(dbo.Context)
                      ?? new Dictionary<string, object>();

        return new Form
        {
            Id = dbo.Id,
            PaTypeId = dbo.PaTypeId,
            Status = (FormStatus)dbo.Status,
            CreationDate = dbo.CreationDate,
            UpdateDate = dbo.UpdateDate,
            Context = context,
            TemplateSnapshot = dbo.TemplateSnapshot
        };
    }
}