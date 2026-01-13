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
            AssigneeId = 1,
            FormDate = DateTime.UtcNow.Date,
            Product = new ProductContextRequest
            {
                ProductId = 1,
                CycleTime = 72,
                WorkstationCapacity = null,
                DailyRate = 400
            },
            Products = null,
            OperationOrProduct = null
        };
}