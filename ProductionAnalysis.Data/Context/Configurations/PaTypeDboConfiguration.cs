using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Context.Configurations;

public class PaTypeDboConfiguration : IEntityTypeConfiguration<PaTypeDbo>
{
    public void Configure(EntityTypeBuilder<PaTypeDbo> builder)
    {
        builder.ToTable("pa_types");
        
        builder.HasKey(e => e.Id);
    }
}