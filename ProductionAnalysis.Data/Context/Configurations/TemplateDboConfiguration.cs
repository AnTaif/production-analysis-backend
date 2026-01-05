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
            .UsingEntity(j => j.ToTable("templates_indicators"));
    }
}