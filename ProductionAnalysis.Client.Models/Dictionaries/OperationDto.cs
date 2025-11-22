namespace ProductionAnalysis.Client.Models.Dictionaries;

public record OperationDto(
    int Id,
    string Name,
    TimeSpan? Duration,
    OperationBasedOnType BasedOnType,
    int? BasedOperationId,
    int? BasedProductId
);