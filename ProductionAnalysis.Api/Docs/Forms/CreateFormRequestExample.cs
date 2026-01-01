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
            Context = new Dictionary<string, object>()
            {
                ["product"] = new ProductContext
                {
                    ProductId = 1,
                    CycleTime = 60,
                    WorkstationCapacity = null,
                    DailyRate = 400
                }
            }
        };

    private record ProductContext
    {
        public int ProductId { get; init; }
        public int? CycleTime { get; init; }
        public int? WorkstationCapacity { get; init; }
        public int DailyRate { get; init; }
    }
}