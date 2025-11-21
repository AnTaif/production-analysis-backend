using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionAnalysis.Data.Models.Dictionaries;

public class OperationDbo
{
    public int Id { get; set; }
    
    [MaxLength(255)]
    public required string Name { get; set; }
    
    public int? TimeToCompleteInSeconds { get; set; }
    
    public OperationBasedOnType OperationBasedOn { get; set; }
    
    public int? BasedOperationId { get; set; }
    
    [ForeignKey(nameof(BasedOperationId))]
    public virtual OperationDbo? BasedOperation { get; set; }
    
    public int? BasedProductId { get; set; }
    
    [ForeignKey(nameof(BasedProductId))]
    public virtual ProductDbo? BasedProduct { get; set; }
}