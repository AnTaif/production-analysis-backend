using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionAnalysis.Data.Models.Dictionaries;

namespace ProductionAnalysis.Data.Context.Configurations;

public class AuxiliaryOperationDboConfiguration : IEntityTypeConfiguration<AuxiliaryOperationDbo>
{
    public void Configure(EntityTypeBuilder<AuxiliaryOperationDbo> builder)
    {
        builder.ToTable("auxiliary_operations");

        builder.HasKey(x => x.Id);
    }
}