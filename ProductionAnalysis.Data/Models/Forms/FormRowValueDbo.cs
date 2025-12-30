namespace ProductionAnalysis.Data.Models.Forms;

public class FormRowValueDbo
{
    public int Id { get; set; }

    public int FormRowId { get; set; }

    public required string FieldKey { get; set; }

    public required string Value { get; set; }

    public string? ValueType { get; set; }

    public FormRowDbo FormRow { get; set; } = null!;
}