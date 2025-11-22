using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Data.Models.Dictionaries;

public class PaTypeDbo
{
    public int Id { get; set; }
    
    [MaxLength(255)]
    public required string Name { get; set; }
}