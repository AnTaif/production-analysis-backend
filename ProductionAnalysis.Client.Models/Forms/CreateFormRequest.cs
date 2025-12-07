namespace ProductionAnalysis.Client.Models.Forms;

public record CreateFormRequest(
    int PaTypeId,
    Dictionary<string, object> Context
);