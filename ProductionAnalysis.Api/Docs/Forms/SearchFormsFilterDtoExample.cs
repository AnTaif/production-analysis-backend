using ProductionAnalysis.Client.Models.Forms;
using Swashbuckle.AspNetCore.Filters;

namespace ProductionAnalysis.Api.Docs.Forms;

public class SearchFormsFilterDtoExample : IExamplesProvider<SearchFormsFilterDto>
{
    public SearchFormsFilterDto GetExamples()
    {
        return new SearchFormsFilterDto
        {
            DepartmentId = null,
            Status = FormStatus.Completed,
            PageNumber = 1,
            PageSize = 100
        };
    }
}