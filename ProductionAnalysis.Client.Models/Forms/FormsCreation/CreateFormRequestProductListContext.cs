namespace ProductionAnalysis.Client.Models.Forms.FormsCreation;

public class CreateFormRequestProductListContext : CreateFormRequestContextBase
{
    public required ICollection<CreateFormRequestProductContext> Products { get; set; }
}