using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Context.Configurations;

public class PositionDboConfiguration : IEntityTypeConfiguration<PositionDbo>
{
    public void Configure(EntityTypeBuilder<PositionDbo> builder)
    {
        builder.ToTable("positions");

        builder.HasKey(p => p.Id);
    }
}