using ProductionAnalysis.Application.Domain.Forms;
using ProductionAnalysis.Data.Models.Forms;

namespace ProductionAnalysis.Data.Converters;

public static class FormsConverter
{
    public static Form ToDomain(this FormDbo dbo)
    {
        return new Form
        {
            Id = dbo.Id,
            PaTypeId = dbo.PaTypeId,
            Status = (FormStatus)dbo.Status,
            CreationDate = dbo.CreationDate,
            UpdateDate = dbo.UpdateDate
        };
    }
}