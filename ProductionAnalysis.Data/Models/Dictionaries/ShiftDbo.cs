using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Data.Models.Dictionaries;

public class ShiftDbo
{
    public int Id { get; set; }
    
    [MaxLength(255)]
    public required string Name { get; set; }
    
    public TimeOnly StartTime { get; set; }
}