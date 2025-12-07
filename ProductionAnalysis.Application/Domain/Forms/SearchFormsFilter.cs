namespace ProductionAnalysis.Application.Domain.Forms;

public class SearchFormsFilter
{
    public int? DepartmentId { get; set; }
    public FormStatus? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}