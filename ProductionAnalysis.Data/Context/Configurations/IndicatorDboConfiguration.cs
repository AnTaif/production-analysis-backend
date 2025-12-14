using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Context.Configurations;

public class IndicatorDboConfiguration : IEntityTypeConfiguration<IndicatorDbo>
{
    public void Configure(EntityTypeBuilder<IndicatorDbo> builder)
    {
        builder.ToTable("indicators");

        builder.HasKey(e => e.Id);
    }
}