using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Context.Configurations;

public class ShiftDboConfiguration : IEntityTypeConfiguration<ShiftDbo>
{
    public void Configure(EntityTypeBuilder<ShiftDbo> builder)
    {
        builder.ToTable("shifts");
        
        builder.HasKey(e => e.Id);
    }
}