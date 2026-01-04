namespace ProductionAnalysis.Client.Models.Forms;

public record FormContextDto
{
    public ProductContextDto? Product { get; init; }
    public ICollection<ProductContextDto>? Products { get; init; }
    public OperationContextDto? Operation { get; init; }
}