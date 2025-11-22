using System.ComponentModel.DataAnnotations;

namespace ProductionAnalysis.Data.Models.Dictionaries;

public class DowntimeReasonGroupDbo
{
    public int Id { get; set; }
    
    [MaxLength(255)]
    public required string Name { get; set; }
    
    [MaxLength(300)]
    public required string Description { get; set; }
}