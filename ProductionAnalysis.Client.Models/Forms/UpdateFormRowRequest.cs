namespace ProductionAnalysis.Client.Models.Forms;

public record UpdateFormRowRequest(
    int FormId,
    short RowOrder,
    Dictionary<int, object> Values // Key - IndicatorId, Value - новое значение
);