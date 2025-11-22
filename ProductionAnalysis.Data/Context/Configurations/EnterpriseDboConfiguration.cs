using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Context.Configurations;

public class EnterpriseDboConfiguration : IEntityTypeConfiguration<EnterpriseDbo>
{
    public void Configure(EntityTypeBuilder<EnterpriseDbo> builder)
    {
        builder.ToTable("enterprises");
        
        builder.HasKey(e => e.Id);
    }
}