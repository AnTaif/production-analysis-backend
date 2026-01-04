using ProductionAnalysis.Application.Domain.Templates;

namespace ProductionAnalysis.Application.Domain.Forms;

public class Form
{
    public Form(
        int id,
        int paTypeId,
        FormStatus status,
        DateTime creationDate,
        DateTime updateDate,
        Dictionary<string, FormContext> context,
        Template templateSnapshot,
        ICollection<FormRow> rows,
        Guid creatorId,
        int shiftId,
        int departmentId,
        Dictionary<int, object>? totalValues = null)
    {
        Id = id;
        PaTypeId = paTypeId;
        Status = status;
        CreationDate = creationDate;
        UpdateDate = updateDate;
        Context = context;
        TemplateSnapshot = templateSnapshot;
        Rows = rows;
        CreatorId = creatorId;
        ShiftId = shiftId;
        DepartmentId = departmentId;
        TotalValues = totalValues;
    }

    public int Id { get; }
    public int PaTypeId { get; }
    public FormStatus Status { get; }
    public DateTime CreationDate { get; }
    public DateTime UpdateDate { get; }
    public Dictionary<string, FormContext> Context { get; }
    public Template TemplateSnapshot { get; }
    public ICollection<FormRow> Rows { get; }
    public Guid CreatorId { get; }
    public int ShiftId { get; }
    public int DepartmentId { get; }
    public Dictionary<int, object>? TotalValues { get; }
}