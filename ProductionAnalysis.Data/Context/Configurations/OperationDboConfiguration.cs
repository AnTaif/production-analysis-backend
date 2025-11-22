using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Context.Configurations;

public class OperationDboConfiguration : IEntityTypeConfiguration<OperationDbo>
{
    public void Configure(EntityTypeBuilder<OperationDbo> builder)
    {
        builder.ToTable("operations");

        builder.HasKey(e => e.Id);

        builder.HasOne<OperationDbo>()
            .WithMany()
            .HasForeignKey(e => e.BasedOperationId);

        builder.HasOne<ProductDbo>()
            .WithMany()
            .HasForeignKey(e => e.BasedProductId);
    }
}