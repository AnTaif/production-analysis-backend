using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Context.Configurations;

public class DowntimeReasonGroupDboConfiguration : IEntityTypeConfiguration<DowntimeReasonGroupDbo>
{
    public void Configure(EntityTypeBuilder<DowntimeReasonGroupDbo> builder)
    {
        builder.ToTable("downtime_reason_groups");
        
        builder.HasKey(e => e.Id);
    }
}