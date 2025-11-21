using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionAnalysis.Data.Models.Dictionaries;

public class ProductDbo
{
    public int Id { get; set; }
    
    [MaxLength(255)]
    public required string Name { get; set; }
    
    public int TactTimeInSeconds { get; set; }
    
    public int EnterpriseId { get; set; }
    
    [ForeignKey(nameof(EnterpriseId))]
    public virtual EnterpriseDbo? Enterprise { get; set; }
}