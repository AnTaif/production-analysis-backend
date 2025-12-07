namespace ProductionAnalysis.Client.Models.Forms;

public record FormRowDto(
    short Order,
    bool IsAdditionalOperation,
    Dictionary<string, object> Values
);