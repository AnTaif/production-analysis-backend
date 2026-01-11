using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Context.Configurations;

public class TemplateDboConfiguration : IEntityTypeConfiguration<TemplateDbo>
{
    public void Configure(EntityTypeBuilder<TemplateDbo> builder)
    {
        builder.ToTable("templates");

        builder.HasKey(e => e.Id);

        builder.HasMany(e => e.Indicators)
            .WithMany(e => e.Templates)
            .UsingEntity<TemplateIndicatorDbo>(
                j => j
                    .HasOne(ti => ti.Indicator)
                    .WithMany()
                    .HasForeignKey(ti => ti.IndicatorId),
                j => j
                    .HasOne(ti => ti.Template)
                    .WithMany()
                    .HasForeignKey(ti => ti.TemplateId),
                j =>
                {
                    j.ToTable("templates_indicators");
                    j.HasKey(ti => new { ti.TemplateId, ti.IndicatorId });
                });
    }
}