using ProductionAnalysis.Client.Models.Forms;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Forms;

public class CreateFormRequestExample : IExamplesProvider<CreateFormRequest>
{
    public CreateFormRequest GetExamples() =>
        new()
        {
            PaTypeId = 1,
            ShiftId = 1,
            Product = new ProductContextDto
            {
                ProductId = 1,
                CycleTime = 60,
                WorkstationCapacity = null,
                DailyRate = 400
            },
            Operation = null
        };
}

public class FormDtoExample : IExamplesProvider<FormDto>
{
    public FormDto GetExamples()
    {
        return new FormDto
        {
            Id = 10,
            PaTypeId = 1,
            Status = FormStatus.InProgress,
            CreationDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow,
            Context = new FormContextDto
            {
                Product = new ProductContextDto
                {
                    ProductId = 1,
                    CycleTime = 60,
                    WorkstationCapacity = null,
                    DailyRate = 400
                },
                Operation = null
            },
            Rows = new List<FormRowDto>
            {
            },
            Template = new FormTemplateDto
            {
            }
        };
    }
}