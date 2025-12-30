namespace ProductionAnalysis.Client.Models.Forms;

public record CreateFormRequest(
    int PaTypeId,
    int ShiftId,
    Dictionary<string, object> Context
);