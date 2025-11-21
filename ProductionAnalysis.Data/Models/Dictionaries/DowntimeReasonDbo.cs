using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionAnalysis.Data.Models.Dictionaries;

public class DowntimeReasonDbo
{
    public int Id { get; set; }
    
    [MaxLength(255)]
    public required string Name { get; set; }
    
    public int DowntimeReasonGroupId { get; set; }
    
    [ForeignKey(nameof(DowntimeReasonGroupId))]
    public virtual DowntimeReasonGroupDbo? DowntimeReasonGroup { get; set; }
}