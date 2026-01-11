using ProductionAnalysis.Client.Models.Forms;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Forms;

public class CreateFormRequestExample : IExamplesProvider<CreateFormRequest>
{
    public CreateFormRequest GetExamples() =>
        new()
        {
            PaType = PaTypeDto.SingleProductWithCycleTime,
            ShiftId = 1,
            ExecutorId = 1,
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