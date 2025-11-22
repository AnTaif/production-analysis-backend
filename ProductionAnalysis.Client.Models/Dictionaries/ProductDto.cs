namespace ProductionAnalysis.Client.Models.Dictionaries;

public record ProductDto(
    int Id,
    string Name,
    TimeSpan TactTime,
    int EnterpriseId
);