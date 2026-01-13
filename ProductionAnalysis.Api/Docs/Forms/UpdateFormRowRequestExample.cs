using ProductionAnalysis.Client.Models.Forms;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Forms;

public class UpdateFormRowRequestExample : IExamplesProvider<UpdateFormRowRequest>
{
    public UpdateFormRowRequest GetExamples()
    {
        return new UpdateFormRowRequest
        {
            Values = new Dictionary<int, object>
            {
                { 3, 50 },
                { 6, "Недостаточно сырья" },
                { 7, 1 },
                { 8, "Заказано дополнительное сырье" }
            }
        };
    }
}