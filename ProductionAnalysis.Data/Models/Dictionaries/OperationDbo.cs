using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Data.Models.Dictionaries;

public class OperationDbo
{
    public int Id { get; set; }
    
    [MaxLength(255)]
    public required string Name { get; set; }
    
    public int? DurationInSeconds { get; set; }
    
    public int BasedOnType { get; set; }
    
    public int? BasedOperationId { get; set; }
    
    public int? BasedProductId { get; set; }
}