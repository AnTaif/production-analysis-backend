using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Context.Configurations;

public class ProductDboConfiguration : IEntityTypeConfiguration<ProductDbo>
{
    public void Configure(EntityTypeBuilder<ProductDbo> builder)
    {
        builder.ToTable("products");
        
        builder.HasKey(e => e.Id);

        builder.HasOne<EnterpriseDbo>()
            .WithMany()
            .HasForeignKey(e => e.EnterpriseId);
    }
}